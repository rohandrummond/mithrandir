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
                    Tier = match.Tier
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

    // public async Task<AddToWhitelistResponse> AddToWhitelistAsync(AddToWhitelistRequest request)
    // {
    //     // TO DO
    // }
    //
    // public async Task<RemoveFromWhitelistResponse> RemoveFromWhitelistAsync(RemoveFromWhitelistRequest request)
    // {
    //     // TO DO
    // }
}