using Microsoft.AspNetCore.Mvc.Testing;

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
    }

    [Fact]
    public async Task AddToWhitelist_WithoutRequiredFields_ReturnsBadRequest()
    {
        // Arrange, act, assert
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