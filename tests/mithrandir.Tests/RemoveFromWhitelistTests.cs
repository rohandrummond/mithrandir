using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class RemoveFromWhitelistTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public RemoveFromWhitelistTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RemoveFromWhitelistRequest
        {
            Id = 999,
            IpAddress = TestIp
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(request)
        });
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    

    [Fact]
    public async Task RemoveFromWhitelist_WithoutKey_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var requestBody = new
        {
            IpAddress = TestIp
        };
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(requestBody)
        };

        // Act
        var response = await _client.SendAsync(request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveFromWhitelist_WithoutIp_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var requestBody = new
        {
            Key = "Without IP Remove From Whitelist Test Key",
        };
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(requestBody)
        };

        // Act
        var response = await _client.SendAsync(request);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithInvalidIp_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var requestBody = new RemoveFromWhitelistRequest
        {
            Id = 999,
            IpAddress = "999.999.999.999"
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(requestBody)
        });
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(result);
        Assert.Contains("IpAddress", result.Errors.Keys);
        Assert.Contains("Invalid IP address", result.Errors["IpAddress"].First());
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithNonExistentIp_ReturnsMessage()
    {
        // Arrange

        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Non Existent IP Remove From Whitelist Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Create remove request for IP
        var removeRequestBody = new RemoveFromWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };

        // Act
        var removeResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(removeRequestBody)
        });
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
        var result = await removeResponse.Content.ReadFromJsonAsync<RemoveFromWhitelistResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithValidIp_ReturnsSuccess()
    {
        // Arrange

        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Valid Remove From Whitelist Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Add IP to whitelist first
        var addToWhitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        var addResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", addToWhitelistRequest);
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        // Create remove request
        var removeRequest = new RemoveFromWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/whitelist/remove")
        {
            Content = JsonContent.Create(removeRequest)
        });
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert

        // Verify response from server
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var removeResult = await response.Content.ReadFromJsonAsync<RemoveFromWhitelistResponse>();
        Assert.NotNull(removeResult);
        Assert.True(removeResult.Success);
        Assert.NotNull(removeResult.WhitelistedIps);
        Assert.DoesNotContain(TestIp, removeResult.WhitelistedIps);
        
        // Verify that subsequent request from IP is rejected
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var validateRequest = new ValidateKeyRequest
        {
            Key = generateKeyResult.Key,
        };
        var validateResponse = await _client.PostAsJsonAsync("/api/keys/validate", validateRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, validateResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

}