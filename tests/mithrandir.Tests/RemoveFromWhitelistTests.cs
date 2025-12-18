using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class RemoveFromWhitelistTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public RemoveFromWhitelistTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithoutAdminKey_ReturnsUnauthorized()
    {
        //
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithoutKey_ReturnsBadRequest()
    {
        //
    }
    
    [Fact]
    public async Task RemoveFromWhitelist_WithoutIp_ReturnsBadRequest()
    {
        //
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithInvalidIp_ReturnsBadRequest()
    {
        //
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithNonExistentIp_ReturnsMessage()
    {
        //        
    }

    [Fact]
    public async Task RemoveFromWhitelist_WithValidIp_ReturnsSuccess()
    {
        //
    }
    
}