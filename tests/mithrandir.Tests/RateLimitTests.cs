using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using mithrandir.Models;
using mithrandir.Models.DTOs;
using StackExchange.Redis;

namespace mithrandir.Tests;

public class RateLimitTests : IClassFixture<RateLimitTestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly RateLimitTestWebApplicationFactory _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;

    public RateLimitTests(RateLimitTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Clean up function to run on every test
    public async Task InitializeAsync()
    {
        await ClearRateLimitKeys();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    
    private async Task ClearRateLimitKeys()
    {
        using var scope = _factory.Services.CreateScope();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var server = redis.GetServer("localhost", 6379);
        var db = redis.GetDatabase();

        await foreach (var key in server.KeysAsync(database: 15, pattern: "rateLimit:*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }

    [Fact]
    public async Task Requests_WithinLimit_Succeed()
    {
        // Arrange
        
        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Rate Limit Within Limit Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Create request for validate endpoint
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var request = new ValidateKeyRequest { Key = generateKeyResult.Key };

        // Act
        for (int i = 0; i < _factory.TestRateLimit; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/keys/validate", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    [Fact]
    public async Task Requests_ExceedingLimit_ReturnsTooManyRequestsAndResets()
    {
        // Arrange
        
        // Generate key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Rate Limit Exceeding Limit Test Key",
            Tier = Tier.Free
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Create validate request
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var request = new ValidateKeyRequest { Key = generateKeyResult.Key };

        // Exhaust rate limit
        for (int i = 0; i < _factory.TestRateLimit; i++)
        {
            await _client.PostAsJsonAsync("/api/keys/validate", request);
        }

        // Make another request
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<RateLimitError>();
        Assert.NotNull(error);
        Assert.Equal("Rate limit exceeded", error.Error);
        Assert.True(response.Headers.Contains("Retry-After"));
        var retryAfter = response.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.NotNull(retryAfter);
        Assert.True(int.TryParse(retryAfter, out var seconds));
        Assert.True(seconds > 0);
        
        // Advance time past the current window
        _factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(11));
        
        // Make request in new time window
        var newResponse = await _client.PostAsJsonAsync("/api/keys/validate", request);
        Assert.Equal(HttpStatusCode.OK, newResponse.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    [Fact]
    public async Task Requests_ProTier_ExceedingLimit_ReturnsTooManyRequestsAndResets()
    {
        // Arrange

        // Generate Pro tier key
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Rate Limit Pro Tier Test Key",
            Tier = Tier.Pro
        };
        var generateKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, generateKeyResponse.StatusCode);
        var generateKeyResult = await generateKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generateKeyResult);

        // Add test IP to whitelist
        var whitelistRequest = new AddToWhitelistRequest
        {
            Id = generateKeyResult.Id,
            IpAddress = TestIp
        };
        await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Create validate request
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);
        var request = new ValidateKeyRequest { Key = generateKeyResult.Key };

        // Exhaust rate limit
        for (int i = 0; i < _factory.TestRateLimit; i++)
        {
            await _client.PostAsJsonAsync("/api/keys/validate", request);
        }

        // Make another request
        var response = await _client.PostAsJsonAsync("/api/keys/validate", request);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<RateLimitError>();
        Assert.NotNull(error);
        Assert.Equal("Rate limit exceeded", error.Error);
        Assert.True(response.Headers.Contains("Retry-After"));
        var retryAfter = response.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.NotNull(retryAfter);
        Assert.True(int.TryParse(retryAfter, out var seconds));
        Assert.True(seconds > 0);

        // Advance time past the current window
        _factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(11));

        // Make request in new time window
        var newResponse = await _client.PostAsJsonAsync("/api/keys/validate", request);
        Assert.Equal(HttpStatusCode.OK, newResponse.StatusCode);

        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }
}