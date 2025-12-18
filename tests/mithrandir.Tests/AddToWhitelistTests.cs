using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
            Key = "Without Admin Key Add To Whitelist Test Key",
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
        // Arrange, act, assert
    }

    [Fact]
    public async Task AddToWhitelist_WithDuplicateIp_ReturnsMessage()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task AddToWhitelist_WithValidIp_ReturnsSuccess()
    {
        // Arrange, act assert
    }
    
}