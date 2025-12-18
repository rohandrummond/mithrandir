using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class GenerateKeyTests : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public GenerateKeyTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task GenerateKey_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new GenerateKeyRequest
        {
            Name = "Without Admin Key Test Key",
            Tier = Tier.Free
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/generatekey", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    }

    [Fact]
    public async Task GenerateKey_WithInvalidAdminKey_ReturnsUnauthorized()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task GenerateKey_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange, act, assert
    }
    
    [Fact]
    public async Task GenerateKey_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task GenerateKey_WithExpiryInPast_ReturnsBadRequest()
    {
        // Arrange, act, assert
    }
    
    [Fact]
    public async Task GenerateKey_WithProTier_ReturnsCorrectTier()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task GenerateKey_ReturnsKeyWithPrefix()
    {
        // Arrange, act, assert
    }
}