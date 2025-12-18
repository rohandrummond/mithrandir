using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Tests;

public class DeleteKeyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIp = CustomWebApplicationFactory.TestIp;
    
    public DeleteKeyTests(CustomWebApplicationFactory factory)
    { 
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task DeleteKey_DeletesKeySuccessfully()
    {
        // Arrange
        
        // Create authentication key for post-deletion check
        _client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        var authKeyRequest = new GenerateKeyRequest
        {
            Name = "With Prefix Generate Key Test Key",
            Tier = Tier.Free
        };
        var authKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", authKeyRequest);
        Assert.Equal(HttpStatusCode.OK, authKeyResponse.StatusCode);
        var authKeyResult = await authKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(authKeyResult);
        
        // Add IP to whitelist 
        var addToWhitelistRequest = new AddToWhitelistRequest
        {
            Key = authKeyResult.Key,
            IpAddress = TestIp,
        };
        var addToWhitelistResponse = await _client.PostAsJsonAsync("/api/admin/keys/whitelist/add", addToWhitelistRequest);
        Assert.Equal(HttpStatusCode.OK, addToWhitelistResponse.StatusCode);
        var addToWhitelistResult = await addToWhitelistResponse.Content.ReadFromJsonAsync<AddToWhitelistResponse>();
        Assert.NotNull(addToWhitelistResult);
        Assert.True(addToWhitelistResult.Success);
        Assert.NotNull(addToWhitelistResult.WhitelistedIps);
        Assert.Contains(TestIp, addToWhitelistResult.WhitelistedIps);
        
        // Create deletion test key
        var testKeyRequest = new GenerateKeyRequest
        {
            Name = "With Prefix Generate Key Test Key",
            Tier = Tier.Free
        };
        var testKeyResponse = await _client.PostAsJsonAsync("/api/admin/keys/generate", testKeyRequest);
        Assert.Equal(HttpStatusCode.OK, testKeyResponse.StatusCode);
        var testKeyResult = await testKeyResponse.Content.ReadFromJsonAsync<GenerateKeyResponse>();
        Assert.NotNull(testKeyResult);

        // Create deletion request object
        var deleteKeyRequestBody = new DeleteKeyRequest
        {
            Key = testKeyResult.Key,
        };
        
        // Act 
        var deleteKeyRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/admin/keys/delete")
        {
            Content = JsonContent.Create(deleteKeyRequestBody)
        };
        var deleteKeyResponse = await _client.SendAsync(deleteKeyRequest);   
        Assert.Equal(HttpStatusCode.OK, deleteKeyResponse.StatusCode);
        _client.DefaultRequestHeaders.Remove("X-Admin-Key");
        
        // Assert
        
        // Check response
        var deleteKeyResult = await deleteKeyResponse.Content.ReadFromJsonAsync<DeleteKeyResponse>();

        Assert.NotNull(deleteKeyResult);
        Assert.True(deleteKeyResult.Success);
        
        _client.DefaultRequestHeaders.Add("X-Api-Key", authKeyResult.Key);
        
        // Validate key has actually been deleted from database
        var validateKeyRequest = new ValidateKeyRequest
        {
            Key = testKeyResult.Key,
        };
        var validateKeyResponse = await _client.PostAsJsonAsync("/api/keys/validate", validateKeyRequest);
        Assert.Equal(HttpStatusCode.OK, validateKeyResponse.StatusCode);
        var validateKeyResult = await validateKeyResponse.Content.ReadFromJsonAsync<ValidateKeyResponse>();
        Assert.NotNull(validateKeyResult);
        Assert.False(validateKeyResult.IsValid);
    }
}