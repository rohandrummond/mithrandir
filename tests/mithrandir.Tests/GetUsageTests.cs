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
        // Arrange, act, assert
    }

    [Fact]
    public async Task GetUsage_WithoutRequestKey_ReturnsUnauthorized()
    {
        // Arrange, act, assert
    }

    [Fact]
    public async Task GetUsage_WithValidRequest_ReturnsCorrectData()
    {
        // Arrange, act, assert
        
        // Before fetching usage simulate the below requests (for asserting usage data)
        // 1x /api/keys/validate with incorrect request body to simulate 401
        // 2x /api/keys/validate with valid request to simulate 200
    }
}