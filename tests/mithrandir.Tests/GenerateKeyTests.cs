using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class GenerateKeyTests : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public GenerateKeyTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task GenerateKey_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new GenerateKeyRequest
        {
            Name = "Without Admin Key Test Key",
            Tier = Tier.Free
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    }

    [Fact]
    public async Task GenerateKey_WithInvalidAdminKey_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "invalid-test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "With invalid Admin Key Test",
            Tier = Tier.Free
        };
        
        // Act 
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    }

    [Fact]
    public async Task GenerateKey_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "",
            Tier = Tier.Free
        };
        
        // Act 
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GenerateKey_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name =
                "The Grey Pilgrim walked unseen among the wary, a keeper of quiet hope and ancient fire, nudging small hands toward great deeds while the world turned on the edge of courage.\n",
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateKey_WithExpiryDate_Succeeds()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "With Expiry Date Generate Key Test Key",
            Tier = Tier.Free,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        var result = await response.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task GenerateKey_WithExpiryInPast_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "Past Expiry Date Generate Key Test Key",
            Tier = Tier.Free,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GenerateKey_WithProTier_ReturnsCorrectTier()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "With Pro Tier Generate Key Test Key",
            Tier = Tier.Pro
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert 
        var result = await response.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(Tier.Pro, result.Tier);
    }

    [Fact]
    public async Task GenerateKey_ReturnsKeyWithPrefix()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new GenerateKeyRequest
        {
            Name = "With Prefix Generate Key Test Key",
            Tier = Tier.Free
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/generate", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        var result = await response.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.StartsWith("mk_", result.Key);
    }
}