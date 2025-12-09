using Microsoft.EntityFrameworkCore;
using System.Data.Common;

using mithrandir.Models;
using mithrandir.Models.DTOs;
using mithrandir.Data;

using System.Security.Cryptography;

namespace mithrandir.Services;

public class ApiKeyService(MithrandirDbContext context) : IApiKeyService
{
    // Store DbContext arg
    private readonly MithrandirDbContext _context = context;

    private async Task<ApiKey?> FindKeyAsync(string key, bool activeOnly = false)
    {
        var query = _context.ApiKeys.AsQueryable();

        if (activeOnly)
        {
            query = query
                .Where(k => k.Status == Status.Active)
                .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTimeOffset.UtcNow);
        }

        var keys = await query.ToListAsync();
        
        return keys.FirstOrDefault(k => BCrypt.Net.BCrypt.Verify(key, k.KeyHash));
    }

    public async Task<GenerateKeyResponse> GenerateKeyAsync(GenerateKeyRequest request)
    {

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
            ExpiresAt = request.ExpiresAt,
        };

        try 
        {
            // Save to database
            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();
            
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
            throw new InvalidOperationException("Failed to save API key to database", ex);
        }
        catch (Exception ex)
        {
            // Handle other errors
            throw new InvalidOperationException("An unexpected error occurred while generating API key", ex);
        }

    }

    public async Task<ValidateKeyResult> ValidateKeyAsync(ValidateKeyRequest request)
    {
        try {
            // Search for key
            var match = await FindKeyAsync(request.Key, true);

            // Send response
            if (match != null)
            {
                return new ValidateKeyResult
                {
                    IsValid = true,
                    Id = match.Id,
                    Tier = match.Tier,
                    IpWhitelist = match.IpWhitelist ?? new List<string>()
                };
            }
            else
            {
                return new ValidateKeyResult
                {
                    IsValid = false,
                    Reason = "Invalid or expired key"
                };
            }
        } 
        catch (DbException ex) 
        {
            throw new InvalidOperationException("Database error while validating key", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while validating key", ex);
        }
    }

    public async Task<RevokeKeyResponse> RevokeKeyAsync(RevokeKeyRequest request)
    {
        try
        {
            // Search for key
            var match = await FindKeyAsync(request.Key, true);

            // Find match and return error if not found
            if (match == null)
            {
                return new RevokeKeyResponse
                {
                    Success = false,
                    Message = "Key not found or already revoked"
                };
            }

            // Update status and save changes
            match.Status = Status.Revoked;
            await _context.SaveChangesAsync();

            // Return response
            return new RevokeKeyResponse
            {
                Success = true,
                Message = "Key has been revoked"
            };
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database error while validating key", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while validating key", ex);
        }
    }

    public async Task<DeleteKeyResponse> DeleteKeyAsync(DeleteKeyRequest request)
    {
        try
        {
            // Search for key
            var match= await FindKeyAsync(request.Key, true);

            // Return error if not found
            if (match == null)
            {
                return new DeleteKeyResponse()
                {
                    Success = false,
                    Message = "Key not found or already deleted"
                };
            }

            // Delete key and save changes
            _context.Remove(match);
            await _context.SaveChangesAsync();

            // Return response
            return new DeleteKeyResponse
            {
                Success = true,
                Message = "Key has been deleted"
            };

        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database error while deleting key", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while deleting key", ex);
        }
    }

    public async Task<AddToWhitelistResponse> AddToWhitelistAsync(AddToWhitelistRequest request)
    {
        try
        {
            // Find key
            var match = await FindKeyAsync(request.Key, true);
            if (match == null)
            {
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
                return new AddToWhitelistResponse
                {
                    Success = false,
                    Message = "IP address already in whitelist"
                };
            }
            
            // Add to whitelist and save
            match.IpWhitelist.Add(request.IpAddress);
            await _context.SaveChangesAsync();

            // Return response
            return new AddToWhitelistResponse
            {
                Success = true,
                WhitelistedIps = match.IpWhitelist
            };

        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database error while adding IP address", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while adding IP address", ex);
        }
    }
    
    public async Task<RemoveFromWhitelistResponse> RemoveFromWhitelistAsync(RemoveFromWhitelistRequest request)
    {
        try
        {
            // Find key
            var match = await FindKeyAsync(request.Key, true);
            if (match == null)
            {
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "Key not found or already removed"
                };
            }
            
            // Check whitelist exists
            if (match.IpWhitelist == null)
            {
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "IP whitelist has not been configured"
                };
            }

            // Check IP address is in whitelist
            if (!match.IpWhitelist.Contains(request.IpAddress))
            {
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = "IP address not in whitelist"
                };
            }
            
            // Remove from whitelist and save changes
            match.IpWhitelist.Remove(request.IpAddress);
            await _context.SaveChangesAsync();

            return new RemoveFromWhitelistResponse
            {
                Success = true,
                WhitelistedIps = match.IpWhitelist
            };
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database error while removing IP address", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while removing IP address", ex);
        }
    }
}