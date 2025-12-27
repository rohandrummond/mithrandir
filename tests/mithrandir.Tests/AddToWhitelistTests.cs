using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class AddToWhitelistTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public AddToWhitelistTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }
    
    [Fact]
    public async Task AddToWhitelist_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Arrange, act, assert
        var request = new AddToWhitelistRequest
        {
            Id = 999,
            IpAddress = TestIp
        };

        // Act
        var response = await  _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        
        // Assert 
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddToWhitelist_WithoutKey_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new
        {
            IpAddress = TestIp
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task AddToWhitelist_WithoutIp_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new
        {
            Key = "Without Admin Key Add To Whitelist Test Key",
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToWhitelist_WithInvalidIp_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var request = new
        {
            Id = 999,
            IpAddress = "999.999.999.999"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();                                                                
        Assert.NotNull(result);                                                                                                                           
        Assert.Contains("IpAddress", result.Errors.Keys);                                                                                                 
        Assert.Contains("Invalid IP address", result.Errors["IpAddress"].First());    
    }

    [Fact]
    public async Task AddToWhitelist_WithDuplicateIp_ReturnsMessage()
    {
        // Arrange
        
        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Duplicate IP Add To Whitelist Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client .PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>(); 
        Assert.NotNull(generateKeyResult);

        // Add first IP address
        var firstIpRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        var firstIpResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", firstIpRequest);
        Assert.Equal(HttpStatusCode.OK, firstIpResponse.StatusCode);

        // Create request for second IP address
        var secondIpRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };

        // Act
        var secondIpResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", secondIpRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        Assert.Equal(HttpStatusCode.OK, secondIpResponse.StatusCode);
        
        // Assert
        var secondIpResult = await secondIpResponse.Content.ReadFromJsonAsync<AddToWhitelistResponse>();
        Assert.NotNull(secondIpResult);
        Assert.False(secondIpResult.Success);
        Assert.NotNull(secondIpResult.Message);
    }

    [Fact]
    public async Task AddToWhitelist_WithValidIp_ReturnsSuccess()
    {
        // Arrange
        
        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Valid Add To Whitelist Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client .PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>(); 
        Assert.NotNull(generateKeyResult);
        
        // Add to whitelist
        var addToWhitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        
        // Act
        var response  = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", addToWhitelistRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        
        // Verify response from server
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var addToWhitelistResult = await response.Content.ReadFromJsonAsync<AddToWhitelistResponse>();
        Assert.NotNull(addToWhitelistResult);
        Assert.True(addToWhitelistResult.Success);
        Assert.NotNull(addToWhitelistResult.WhitelistedIps);
        
        // Verify subsequent requests succeed
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var validateRequest = new ValidateKeyRequest
        {
            Key = generateKeyResult.Key,
        };
        var validateResponse = await _client.PostAsJsonAsync("/api/keys/validate", validateRequest);
        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }
    
}