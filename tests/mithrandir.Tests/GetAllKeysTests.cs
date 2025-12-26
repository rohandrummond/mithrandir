using System.Net;
using System.Net.Http.Json;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class GetAllKeysTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetAllKeysTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllKeys_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/keys");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllKeys_WithInvalidAdminKey_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "invalid-admin-key");

        // Act
        var response = await _client.GetAsync("/api/admin/keys");
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllKeys_WithValidAdminKey_ReturnsSuccess()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

        // Act
        var response = await _client.GetAsync("/api/admin/keys");
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAllKeysResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Keys);
    }

    [Fact]
    public async Task GetAllKeys_ReturnsCreatedKeys()
    {
        // Arrange 
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

        // Create key
        var createRequest = new GenerateKeyRequest
        {
            Name = "GetAllKeys Test Key",
            Tier = Tier.Pro
        };
        var createResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdKey = await createResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(createdKey);

        // Act
        var response = await _client.GetAsync("/api/admin/keys");
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAllKeysResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Keys);

        // Verify new key is in response
        var matchingKey = result.Keys.Find(k => k.Name == "GetAllKeys Test Key");
        Assert.NotNull(matchingKey);
        Assert.Equal(Tier.Pro, matchingKey.Tier);
        Assert.Equal(Status.Active, matchingKey.Status);
    }

    [Fact]
    public async Task GetAllKeys_DoesNotExposeKeyHash()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

        // Create a key
        var createRequest = new GenerateKeyRequest
        {
            Name = "Hash Security Test Key",
            Tier = Tier.Free
        };
        await _client.PostAsJsonAsync("/api/admin/keys/generate", createRequest);

        // Act
        var response = await _client.GetAsync("/api/admin/keys");
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify response doesn't contain KeyHash
        var rawContent = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("KeyHash", rawContent);
        Assert.DoesNotContain("keyHash", rawContent);
    }
}
