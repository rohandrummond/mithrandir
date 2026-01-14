using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class ZodiacTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string TestIp = CustomWebApplicationFactory.TestIp;

    public ZodiacTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Helper function for setting up an authenticated API key
    private async Task<string> SetupAuthenticatedKey(string keyName)
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");

        // Generate key
        var generateRequest = new GenerateKeyRequest
        {
            Name = keyName,
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
        await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", whitelistRequest);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        return generatedKey.Key;
    }

    [Fact]
    public async Task GetZodiac_WithoutApiKey_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/zodiac?dob=1990-01-01");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetZodiac_WithInvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Api-Key", "mk_invalidkey");

        // Act
        var response = await _client.GetAsync("/api/zodiac?dob=1990-01-01");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetZodiac_WithUnauthorizedIp_ReturnsUnauthorized()
    {
        // Arrange
        
        // Generate key without whitelisting test IP
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateRequest = new GenerateKeyRequest
        {
            Name = "Zodiac Unauthorized IP Test Key",
            Tier = Tier.Free
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", generateRequest);
        var generatedKey = await generateResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(generatedKey);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");

        // Act 
        _client.DefaultRequestHeaders.Add("X-Api-Key", generatedKey.Key);
        var response = await _client.GetAsync("/api/zodiac?dob=1990-01-01");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetZodiac_WithoutDob_ReturnsBadRequest()
    {
        // Arrange
        var apiKey = await SetupAuthenticatedKey("Zodiac No DOB Test Key");
        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        // Act
        var response = await _client.GetAsync("/api/zodiac");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Date of birth is required", content);
    }

    [Fact]
    public async Task GetZodiac_WithInvalidDateFormat_ReturnsBadRequest()
    {
        // Arrange
        var apiKey = await SetupAuthenticatedKey("Zodiac Invalid Date Test Key");
        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        // Act
        var response = await _client.GetAsync("/api/zodiac?dob=not-a-date");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid date format", content);
    }

    [Fact]
    public async Task GetZodiac_WithValidDate_ReturnsZodiacResponse()
    {
        // Arrange
        var apiKey = await SetupAuthenticatedKey("Zodiac Aries Test Key");
        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        // Act (March 25 should return Aries)
        var response = await _client.GetAsync("/api/zodiac?dob=1990-03-25");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Zodiac: Aries", content);
        Assert.Contains("Gimli", content);
    }

    [Fact]
    public async Task GetZodiac_WithLeoBirthday_ReturnsAragorn()
    {
        // Arrange
        var apiKey = await SetupAuthenticatedKey("Zodiac Leo Test Key");
        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        // Act - August 15 = Leo
        var response = await _client.GetAsync("/api/zodiac?dob=1990-08-15");
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Zodiac: Leo", content);
        Assert.Contains("Aragorn", content);
        Assert.Contains("King Elessar", content);
    }
}
