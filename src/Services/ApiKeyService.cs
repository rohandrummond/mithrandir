using Microsoft.EntityFrameworkCore;

using mithrandir.Models;
using mithrandir.Models.DTOs;
using mithrandir.Data;

using System.Security.Cryptography;

namespace mithrandir.Services;

public class ApiKeyService(MithrandirDbContext context) : IApiKeyService
{
    // Store DbContext arg
    private readonly MithrandirDbContext _context = context;

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
}