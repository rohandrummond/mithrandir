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
        // Arrange, act, assert
    }

    [Fact]
    public async Task RevokeKey_WithoutKey_ReturnsBadRequest()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task RevokeKey_WithAlreadyRevokedKey_ReturnsMessage()
    {
        // Arrange, act, assert
    }
    
    [Fact]
    public async Task RevokeKey_WithValidRequest_ReturnsSuccess()
    {
        // Arrange, act, assert
        // Make sure that revoked key can no longer be used for requests
    }

}