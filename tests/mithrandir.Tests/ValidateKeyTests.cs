using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using mithrandir.Data;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class ValidateKeyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public ValidateKeyTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }
      
    [Fact]
    public async Task ValidateKey_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ValidateKeyRequest
        {
            Key = "mk_invalidkey"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
        
        // Assert 
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidateKey_WithoutIpWhitelist_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        
        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = "No IP Whitelist Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", generatedKey.Key);
        var request = new ValidateKeyRequest
        {
            Key = "mk_invalidkey"
        };
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateKey_WithUnauthorizedIp_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        
        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = "Unauthorized IP Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        
        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generatedKey.Id,
            IpAddress = "127.0.0.2"
        };
        var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", generatedKey.Key);
        var request = new ValidateKeyRequest
        {
            Key = "mk_invalidkey"
        };
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidateKey_WithInvalidKey_ReturnsInvalid()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        
        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = "Invalid Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        
        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generatedKey.Id,
            IpAddress = TestIp
        };
        var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", generatedKey.Key);
        var request = new ValidateKeyRequest
        {
            Key = "mk_invalidkey"
        };
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        
        // Assert
        var result = await response.Content.ReadFromJsonAsync<ValidateKeyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateKey_WithValidKey_ReturnsValid()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        
        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = "Valid Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        
        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generatedKey.Id,
            IpAddress = TestIp
        };
        var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", generatedKey.Key);
        var validateRequest = new ValidateKeyRequest
        {
            Key = generatedKey.Key
        };
        var response = await _client.PostAsJsonAsync("/api/keys/validate", validateRequest);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        
        // Assert
        var result = await response.Content.ReadFromJsonAsync<ValidateKeyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal(generateRequest.Tier, result.Tier);
    }

      [Fact]
      public async Task ValidateKey_WithRevokedKey_ReturnsInvalid()
      {
          // Arrange 
          _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

          // Create key for authenticating validate request
          var authKeyRequest = new GenerateKeyRequest
          {
              Name = "Revoke Auth Key",
              Tier = Tier.Free
          };
          var authKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", authKeyRequest);
          var authKey = await authKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
          Assert.NotNull(authKey);
          
          // Create key for testing revoke endpoint
          var testKeyRequest = new GenerateKeyRequest
          {
              Name = "Revoke Test Key",
              Tier = Tier.Free
          };
          var testKeyResponse =  await _client.PostAsJsonAsync("/api/admin/keys/generate", testKeyRequest);
          var testKey = await testKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
          Assert.NotNull(testKey);
          
          // Add IP to whitelist
          var whitelistRequest = new AddToWhitelistRequest
          {
              Id = authKey.Id,
              IpAddress = TestIp
          };
          var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
          Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);

          _client.DefaultRequestHeaders.Remove("X-Admin-Key");

          // Revoke key
          _client.DefaultRequestHeaders.Add("X-Api-Key", authKey.Key);
          var revokeRequest = new RevokeKeyRequest
          {
              Key = testKey.Key
          };
          var revokeResponse = await _client.PatchAsJsonAsync("/api/keys/revoke", revokeRequest);
          Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

          // Act 
          var request = new ValidateKeyRequest
          {
              Key = testKey.Key
          };
          var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
          _client.DefaultRequestHeaders.Remove("X-Api-Key");

          // Assert
          var result = await response.Content.ReadFromJsonAsync<ValidateKeyResponse>();
          Assert.Equal(HttpStatusCode.OK, response.StatusCode);
          Assert.NotNull(result);
          Assert.False(result.IsValid); 
      }
  
    [Fact]
    public async Task ValidateKey_WithExpiredKey_ReturnsInvalid()
    {
      // Arrange 
      _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

      // Create key for authenticating validate request
      var authKeyRequest = new GenerateKeyRequest
      {
          Name = "Expiry Auth Key",
          Tier = Tier.Free
      };
      var authKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", authKeyRequest);
      var authKey = await authKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
      Assert.NotNull(authKey);
      
      // Create key for testing expiration
      var testKeyRequest = new GenerateKeyRequest
      {
          Name = "Expiry Test Key",
          Tier = Tier.Free
      };
      var testKeyResponse =  await _client.PostAsJsonAsync("/api/admin/keys/generate", testKeyRequest);
      var testKey = await testKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
      Assert.NotNull(testKey);
      
      // Add IP to whitelist
      var whitelistRequest = new AddToWhitelistRequest
      {
          Id = authKey.Id,
          IpAddress = TestIp
      };
      var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
      Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);

      _client.DefaultRequestHeaders.Remove("X-Admin-Key");

      // Update expiration for key directly in database
      using (var scope = _factory.Services.CreateScope())
      {
          var db = scope.ServiceProvider.GetRequiredService<MithrandirDbContext>();
          var apiKey = await db.ApiKeys.FirstAsync(k => k.Name == "Expiry Test Key");
          apiKey.ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1);
          await db.SaveChangesAsync();
      }

      // Act
      _client.DefaultRequestHeaders.Add("X-Api-Key", authKey.Key);
      var request = new ValidateKeyRequest
      {
          Key = testKey.Key
      };
      var response = await _client.PostAsJsonAsync("/api/keys/validate", request);
      _client.DefaultRequestHeaders.Remove("X-Api-Key");

      // Assert
      var result = await response.Content.ReadFromJsonAsync<ValidateKeyResponse>();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.NotNull(result); 
      Assert.False(result.IsValid); 
    }
}