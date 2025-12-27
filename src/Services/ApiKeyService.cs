using Microsoft.EntityFrameworkCore;
using System.Data.Common;

using mithrandir.Models;
using mithrandir.Models.DTOs;
using mithrandir.Data;

using System.Security.Cryptography;

namespace mithrandir.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly MithrandirDbContext _context;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(MithrandirDbContext context, ILogger<ApiKeyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<ApiKey?> FindKeyAsync(string key, bool activeOnly = false)
    {
        _logger.LogInformation("Finding API key, activeOnly = {ActiveOnly}", activeOnly);

        // Initialise query
        var query = _context.ApiKeys.AsQueryable();

        // Update query if active only
        if (activeOnly)
        {
            query = query
                .Where(k => k.Status == Status.Active)
                .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTimeOffset.UtcNow);
        }

        // Get keys and find match
        var keys = await query.ToListAsync();
        var match = keys.FirstOrDefault(k => BCrypt.Net.BCrypt.Verify(key, k.KeyHash));
        
        // Log based on whether match is found 
        if (match != null)
        {
            _logger.LogDebug("API key found: ID = {KeyId}, Name = {KeyName}", match.Id, match.Name);
        }
        else
        {
            _logger.LogDebug("API key not found");
        }
    
        // Return match
        return match;
    }

    public async Task<GenerateKeyResponse> GenerateKeyAsync(GenerateKeyRequest request)
    {
        _logger.LogInformation("Generating new API key: Name = {Name}, Tier = {Tier}", 
            request.Name, request.Tier);

        // Generate random key
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes); 
        var key = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 32); 
        var formattedKey = $"mk_{key}";

        // Hash key
        var hash = BCrypt.Net.BCrypt.HashPassword(formattedKey);

        // Create ApiKey object      
        var apiKey = new ApiKey
        {
            KeyHash = hash,
            Name = request.Name,
            Tier = request.Tier,
            Status = Status.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt?.ToUniversalTime(),
        };

        try 
        {
            // Save to database
            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key generated: ID = {KeyId}, Name = {Name}", 
                apiKey.Id, apiKey.Name);
            
            // Return response
            return new GenerateKeyResponse
            {
                Key = formattedKey,
                Name = apiKey.Name,
                Tier = apiKey.Tier,
                CreatedAt = apiKey.CreatedAt,
                ExpiresAt = apiKey.ExpiresAt
            };
        }
        catch (DbUpdateException ex) 
        {
            // Handle database errors
            _logger.LogError(ex, "Database error while generating API key: Name = {Name}", request.Name);
            throw new InvalidOperationException("Failed to save API key to database", ex);
        }
        catch (Exception ex)
        {
            // Handle other errors
            _logger.LogError(ex, "Unexpected error while generating API key: Name = {Name}", request.Name);
            throw new InvalidOperationException("An unexpected error occurred while generating API key", ex);
        }

    }

    public async Task<AuthenticateKeyResponse> AuthenticateKeyAsync(AuthenticateKeyRequest request)
    {
        _logger.LogInformation("Authenticating API key");
        try
        {
            var match = await FindKeyAsync(request.Key, true);

            if (match != null)
            {
                if (match.ExpiresAt != null && match.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation(
                        "API key expired: ID = {KeyId}, Name = {Name}, ExpiresAt = {ExpiresAt}",
                        match.Id, match.Name, match.ExpiresAt);

                    return new AuthenticateKeyResponse
                    {
                        IsValid = false,
                        Reason = "Key expired"
                    };
                }
            
                _logger.LogDebug("API key authenticated successfully: ID = {KeyId}, Name = {Name}",
                    match.Id, match.Name);

                match.LastUsedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();

                // Send response
                return new AuthenticateKeyResponse
                {
                    IsValid = true,
                    Id = match.Id,
                    Tier = match.Tier,
                    IpWhitelist = match.IpWhitelist
                };
            }
            
            _logger.LogWarning("API key authentication failed");
            
            return new AuthenticateKeyResponse
            {
                IsValid = false,
                Reason = "Invalid or expired key"
            };
            
        }
        catch (DbException ex) 
        {
            _logger.LogError(ex, "Database error while authenticating key");
            throw new InvalidOperationException("Database error while authenticating key", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while authenticating key");
            throw new InvalidOperationException("An unexpected error occurred while authenticating key", ex);
        }

    }

    public async Task<ValidateKeyResult> ValidateKeyAsync(ValidateKeyRequest request)
    {
        _logger.LogInformation("Validating API key");
        
        try {
            // Search for key
            var match = await FindKeyAsync(request.Key, true);
            
            if (match != null)
            {
                
                if (match.ExpiresAt != null && match.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation(
                        "API key expired: ID = {KeyId}, Name = {Name}, ExpiresAt = {ExpiresAt}",
                        match.Id, match.Name, match.ExpiresAt);

                    return new ValidateKeyResult
                    {
                        IsValid = false,
                        Reason = "Key expired"
                    };
                }
                
                _logger.LogDebug("API key validated successfully: ID = {KeyId}, Name = {Name}", 
                    match.Id, match.Name);
                
                // Send response
                return new ValidateKeyResult
                {
                    IsValid = true,
                    Id = match.Id,
                    Tier = match.Tier,
                    IpWhitelist = match.IpWhitelist
                };
            }
            
            _logger.LogWarning("API key validation failed");
            
            return new ValidateKeyResult
            {
                IsValid = false,
                Reason = "Invalid or expired key"
            };
            
        } 
        catch (DbException ex) 
        {
            _logger.LogError(ex, "Database error while validating key");
            throw new InvalidOperationException("Database error while validating key", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating key");
            throw new InvalidOperationException("An unexpected error occurred while validating key", ex);
        }
    }

    public async Task<RevokeKeyResponse> RevokeKeyAsync(RevokeKeyRequest request)
    {
        _logger.LogInformation("Revoking API key");
        
        try
        {
            // Search for key
            var match = await FindKeyAsync(request.Key, true);

            // Find match and return error if not found
            if (match == null)
            {
                _logger.LogWarning("Cannot revoke access because key not found or already revoked");

                return new RevokeKeyResponse
                {
                    Success = false,
                    Message = "Key not found or already revoked"
                };
            }

            // Update status and save changes
            match.Status = Status.Revoked;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key revoked: ID = {KeyId}, Name = {Name}", 
                match.Id, match.Name);

            // Return response
            return new RevokeKeyResponse
            {
                Success = true,
                Message = "Key has been revoked"
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while revoking API key");
            throw new InvalidOperationException("Database error while revoking key", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while revoking key");
            throw new InvalidOperationException("Unexpected error while revoking key", ex);
        }
    }

    public async Task<DeleteKeyResponse> DeleteKeyAsync(DeleteKeyRequest request)
    {
        _logger.LogInformation("Attempting to delete API key");

        try
        {
            // Find key by ID
            var match = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.Id);

            // Return error if not found
            if (match == null)
            {
                _logger.LogWarning("Cannot delete API key because key with ID {KeyId} not found", request.Id);

                return new DeleteKeyResponse()
                {
                    Success = false,
                    Message = "Key not found"
                };
            }

            // Delete key and save changes
            _context.Remove(match);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key deleted successfully: ID = {KeyId}", match.Id);

            // Return response
            return new DeleteKeyResponse
            {
                Success = true,
                Message = "Key has been deleted"
            };

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting key");
            throw new InvalidOperationException("Database error while deleting key", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting key");
            throw new InvalidOperationException("Unexpected error while deleting key", ex);
        }
    }

    public async Task<AddToWhitelistResponse> AddToWhitelistAsync(AddToWhitelistRequest request)
    {
        _logger.LogInformation("Adding IP to whitelist: {IpAddress}", request.IpAddress);
        
        try
        {
            // Find key
            var match = await FindKeyAsync(request.Key, true);
            if (match == null)
            {
                _logger.LogWarning("Cannot add IP to whitelist because key not found");
                
                return new AddToWhitelistResponse
                {
                    Success = false,
                    Message = "Key not found"
                };
            }

            // Initialize list if null
            if (match.IpWhitelist == null)
            {
                match.IpWhitelist = new List<string>();
            }

            // Check if already in whitelist
            if (match.IpWhitelist.Contains(request.IpAddress))
            {
                _logger.LogInformation("IP already in whitelist: Key ID = {KeyId}, IP = {IpAddress}", 
                    match.Id, request.IpAddress);
                
                return new AddToWhitelistResponse
                {
                    Success = false,
                    Message = "IP address already in whitelist"
                };
            }
            
            // Add to whitelist and save
            match.IpWhitelist.Add(request.IpAddress);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("IP added to whitelist successfully: Key ID = {KeyId}, IP = {IpAddress}", 
                match.Id, request.IpAddress);

            // Return response
            return new AddToWhitelistResponse
            {
                Success = true,
                Message = "IP address added to whitelist",
                WhitelistedIps = match.IpWhitelist
            };

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding IP to whitelist: IP = {IpAddress}", 
                request.IpAddress);
            throw new InvalidOperationException("Database error while adding IP address", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding IP to whitelist: IP = {IpAddress}", 
                request.IpAddress);
            throw new InvalidOperationException("An unexpected error occurred while adding IP address", ex);
        }
    }
    
    public async Task<RemoveFromWhitelistResponse> RemoveFromWhitelistAsync(RemoveFromWhitelistRequest request)
    {
        _logger.LogInformation("Removing IP from whitelist: {IpAddress}", request.IpAddress);
        
        try
        {
            // Find key
            var match = await FindKeyAsync(request.Key, true);
            if (match == null)
            {
                _logger.LogWarning("Cannot remove IP from whitelist because key not found");

                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "Key not found or already removed"
                };
            }
            
            // Check whitelist exists
            if (match.IpWhitelist == null)
            {
                _logger.LogWarning("Cannot remove from whitelist because whitelist not configured: Key ID = {KeyId}", 
                    match.Id);
                
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "IP whitelist has not been configured"
                };
            }

            // Check IP address is in whitelist
            if (!match.IpWhitelist.Contains(request.IpAddress))
            {
                _logger.LogWarning("Cannot remove because IP is not in whitelist: Key ID = {KeyId}, IP = {IpAddress}", 
                    match.Id, request.IpAddress);
                
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "IP address not in whitelist"
                };
            }
            
            // Remove from whitelist and save changes
            match.IpWhitelist.Remove(request.IpAddress);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("IP removed from whitelist successfully: KeyID = {KeyId}, IP = {IpAddress}", 
                match.Id, request.IpAddress);

            return new RemoveFromWhitelistResponse
            {
                Success = true,
                Message = "IP address removed from whitelist",
                WhitelistedIps = match.IpWhitelist
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while removing IP from whitelist: IP = {IpAddress}", 
                request.IpAddress);
            throw new InvalidOperationException("Database error while removing IP from whitelist", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while removing IP from whitelist: IP = {IpAddress}", 
                request.IpAddress);
            throw new InvalidOperationException("An unexpected error occurred while removing IP from whitelist", ex);
        }
    }

    public async Task<GetAllKeysResponse> GetAllKeysAsync()
    {
        _logger.LogInformation("Retrieving all API keys");

        try
        {
            var keys = await _context.ApiKeys
                .Select(k => new ApiKeyDto
                {
                    Id = k.Id,
                    Name = k.Name,
                    Tier = k.Tier,
                    Status = k.Status,
                    IpWhitelist = k.IpWhitelist,
                    CreatedAt = k.CreatedAt,
                    ExpiresAt = k.ExpiresAt,
                    LastUsedAt = k.LastUsedAt
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} API keys", keys.Count);

            return new GetAllKeysResponse
            {
                Success = true,
                Keys = keys
            };
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database error while retrieving all API keys");
            throw new InvalidOperationException("Database error while retrieving API keys", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all API keys");
            throw new InvalidOperationException("Unexpected error while retrieving API keys", ex);
        }
    }

    public async Task<GetUsageResponse?> GetUsageAsync(GetUsageRequest request)
    {
        _logger.LogInformation("Retrieving usage data for API key");

        try
        {
            // Get entry from ApiKey table
            var match = await FindKeyAsync(request.Key, true);
            if (match == null)
            {
                _logger.LogWarning("Cannot retrieve usage data because key not found");
                return null;
            }
            
            // Get entries from ApiUsage
            var usage = await _context.ApiUsages
                .Where(u => u.ApiKeyId == match.Id)
                .ToListAsync();
            
            _logger.LogInformation("Usage data retrieved: Key ID = {KeyId}, Total Records = {RecordCount}", 
                match.Id, usage.Count);

            // Count total requests
            var totalRequests = usage.Count;
            
            // Count successful requests
            var successfulRequests = usage.Count(u => u.StatusCode >= 200 && u.StatusCode < 300);
            
            // Count failed requests
            var failedRequests = usage.Count(u => u.StatusCode >= 400);
            
            // Group by endpoint 
            var endpointUsage = usage
                .GroupBy(u => u.Endpoint)
                .Select(g => new EndpointUsage
                {
                    Endpoint = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(e => e.Count)
                .ToList();
            
            // Group by status code
            var statusCodeSummary = usage
                .GroupBy(u => u.StatusCode)
                .Select(g => new StatusCodeSummary
                {
                    StatusCode = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(e => e.Count)
                .ToList();
            
            // Send response
            return new GetUsageResponse
            {
                Tier = match.Tier,
                Status = match.Status,
                CreatedAt = match.CreatedAt,
                ExpiresAt = match.ExpiresAt,
                LastUsedAt = match.LastUsedAt,
                TotalRequests = totalRequests, 
                SuccessfulRequests = successfulRequests,  
                FailedRequests = failedRequests,
                EndpointUsage = endpointUsage,
                StatusCodeSummaries = statusCodeSummary
            };
            
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database error while retrieving usage data");
            throw new InvalidOperationException("Database error while retrieving usage data", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving usage data");
            throw new InvalidOperationException("Unexpected error while retrieving usage data", ex);
        }
    }
}