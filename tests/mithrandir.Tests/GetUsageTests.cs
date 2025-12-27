using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class GetUsageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public GetUsageTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task GetUsage_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new GetUsageRequest
        {
            Key = "mk_somekey"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/keys/usage", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsage_WithoutRequestKey_ReturnsUnauthorized()
    {
        // Arrange

        // Generate key for authentication
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Without Request Key Usage Test Auth Key",
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
        var response = await _client.PostAsJsonAsync("/api/keys/usage", request);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsage_WithValidRequest_ReturnsCorrectData()
    {
        // Arrange

        // Generate key for authentication
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var generateKeyRequest = new GenerateKeyRequest
        {
            Name = "Valid Usage Test Auth Key",
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

        // Simulate requests for usage data
        _client.DefaultRequestHeaders.Add("X-Api-Key", generateKeyResult.Key);

        // Simulate 1x failed request
        var invalidValidateRequest = new { };
        var invalidValidateResponse = await _client.PostAsJsonAsync("/api/keys/validate", invalidValidateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidValidateResponse.StatusCode);

        // Simulate 2x successful requests
        var validValidateRequest = new ValidateKeyRequest
        {
            Key = generateKeyResult.Key
        };
        var firstValidateResponse = await _client.PostAsJsonAsync("/api/keys/validate", validValidateRequest);
        Assert.Equal(HttpStatusCode.OK, firstValidateResponse.StatusCode);

        var secondValidateResponse = await _client.PostAsJsonAsync("/api/keys/validate", validValidateRequest);
        Assert.Equal(HttpStatusCode.OK, secondValidateResponse.StatusCode);

        // Act
        var usageRequest = new GetUsageRequest
        {
            Key = generateKeyResult.Key
        };
        var response = await _client.PostAsJsonAsync("/api/keys/usage", usageRequest);
        _client.DefaultRequestHeaders.Remove("X-Api-Key");

        // Assert
        
        // Very successful request
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var usageResult = await response.Content.ReadFromJsonAsync<GetUsageResponse>();
        Assert.NotNull(usageResult);
        
        // Verify key details
        Assert.Equal(Tier.Free, usageResult.Tier);
        Assert.Equal(Status.Active, usageResult.Status);
        Assert.Equal(3, usageResult.TotalRequests);
        Assert.Equal(2, usageResult.SuccessfulRequests);

        // Verify EndpointUsage
        Assert.NotNull(usageResult.EndpointUsage);
        var validateEndpointUsage = usageResult.EndpointUsage.FirstOrDefault(e => e.Endpoint == "/api/keys/validate");
        Assert.NotNull(validateEndpointUsage);
        Assert.Equal(3, validateEndpointUsage.Count);

        // Verify StatusCodeSummaries
        Assert.NotNull(usageResult.StatusCodeSummaries);
        var okStatusSummary = usageResult.StatusCodeSummaries.FirstOrDefault(s => s.StatusCode == 200);
        Assert.NotNull(okStatusSummary);
        Assert.Equal(2, okStatusSummary.Count);
        var badRequestStatusSummary = usageResult.StatusCodeSummaries.FirstOrDefault(s => s.StatusCode == 400);
        Assert.NotNull(badRequestStatusSummary);
        Assert.Equal(1, badRequestStatusSummary.Count);
    }
}