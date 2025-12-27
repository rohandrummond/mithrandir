using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class RevokeKeyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public RevokeKeyTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task RevokeKey_WithoutApiKey_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new RevokeKeyRequest
        {
            Key = "mk_test_key"
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/keys/revoke", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeKey_WithoutKey_ReturnsBadRequest()
    {
        // Arrange

        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Without Key Revoke Test Auth Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Add IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Create request without Key field
        var request = new { };

        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var response = await _client.PatchAsJsonAsync("/api/keys/revoke", request);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RevokeKey_WithAlreadyRevokedKey_ReturnsMessage()
    {
        // Arrange

        // Generate first key for authenticating test
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var authKeyRequest = new GenerateKeyRequest
        {
            Name = "Authentication Revoke Key Test Key",
            Tier = Tier.Free
        };
        var authKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", authKeyRequest);
        Assert.Equal(HttpStatusCode.OK, authKeyResponse.StatusCode);
        var authKeyResult = await authKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(authKeyResult);

        // Add IP to whitelist for auth key
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = authKeyResult.Id,
            IpAddress = TestIp
        };
        var whitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        Assert.Equal(HttpStatusCode.OK, whitelistResponse.StatusCode);

        // Generate key to be revoked
        var revokeKeyRequest = new GenerateKeyRequest
        {
            Name = "Already Revoked Test Key",
            Tier = Tier.Free
        };
        var revokeKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", revokeKeyRequest);
        Assert.Equal(HttpStatusCode.OK, revokeKeyResponse.StatusCode);
        var revokeKeyResult = await revokeKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(revokeKeyResult);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Revoke key 
        _client.DefaultRequestHeaders.Add("X-Api-Key", authKeyResult.Key);
        var firstRevokeRequest = new RevokeKeyRequest
        {
            Key = revokeKeyResult.Key
        };
        var firstRevokeResponse = await _client.PatchAsJsonAsync("/api/keys/revoke", firstRevokeRequest);
        Assert.Equal(HttpStatusCode.OK, firstRevokeResponse.StatusCode);

        // Act
        var secondRevokeRequest = new RevokeKeyRequest
        {
            Key = revokeKeyResult.Key
        };
        var response = await _client.PatchAsJsonAsync("/api/keys/revoke", secondRevokeRequest);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RevokeKeyResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }
    
    [Fact]
    public async Task RevokeKey_WithValidRequest_ReturnsSuccess()
    {
        // Arrange

        // Generate key for authenticating test
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var authKeyRequest = new GenerateKeyRequest
        {
            Name = "Valid Revoke Test Auth Key",
            Tier = Tier.Free
        };
        var authKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", authKeyRequest);
        Assert.Equal(HttpStatusCode.OK, authKeyResponse.StatusCode);
        var authKeyResult = await authKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(authKeyResult);

        // Add IP to whitelist for auth key
        var authWhitelistRequest = new AddToWhitelistRequest
        {
            Id = authKeyResult.Id,
            IpAddress = TestIp
        };
        var authWhitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", authWhitelistRequest);
        Assert.Equal(HttpStatusCode.OK, authWhitelistResponse.StatusCode);

        // Generate key to be revoked
        var revokeKeyRequest = new GenerateKeyRequest
        {
            Name = "Valid Revoke Test Key",
            Tier = Tier.Free
        };
        var revokeKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", revokeKeyRequest);
        Assert.Equal(HttpStatusCode.OK, revokeKeyResponse.StatusCode);
        var revokeKeyResult = await revokeKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(revokeKeyResult);

        // Add IP to whitelist for revoke key
        var revokeWhitelistRequest = new AddToWhitelistRequest
        {
            Id = revokeKeyResult.Id,
            IpAddress = TestIp
        };
        var revokeWhitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", revokeWhitelistRequest);
        Assert.Equal(HttpStatusCode.OK, revokeWhitelistResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Act
        _client.DefaultRequestHeaders.Add("X-Api-Key", authKeyResult.Key);
        var revokeRequest = new RevokeKeyRequest
        {
            Key = revokeKeyResult.Key
        };
        var revokeResponse = await _client.PatchAsJsonAsync("/api/keys/revoke", revokeRequest);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert

        // Verify response from server
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
        var revokeResult = await revokeResponse.Content.ReadFromJsonAsync<RevokeKeyResponse>();
        Assert.NotNull(revokeResult);
        Assert.True(revokeResult.Success);

        // Verify revoked key can no longer be used for requests
        _client.DefaultRequestHeaders.Add("X-Api-Key", revokeKeyResult.Key);
        var unauthorizedRequest = new ValidateKeyRequest
        {
            Key = revokeKeyResult.Key
        };
        var unauthorizedResponse = await _client.PostAsJsonAsync("/api/keys/validate", unauthorizedRequest);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);
    }
}
