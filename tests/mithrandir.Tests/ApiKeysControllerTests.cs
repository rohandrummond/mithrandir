using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mithrandir.Data;
using mithrandir.Models;
using mithrandir.Models.DTOs;
using StackExchange.Redis;

namespace mithrandir.Tests;

public class FakeIpStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                await nextMiddleware();
            });
            next(app);
        };
    }
}

public class ApiKeysControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string TestIp = "127.0.0.1";
    
    public ApiKeysControllerTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
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
    public async Task ValidateKey_WithInvalidKey_ReturnsInvalid()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        
        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = "Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        
        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Key = generatedKey.Key,
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
            Name = "Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        
        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Key = generatedKey.Key,
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
}