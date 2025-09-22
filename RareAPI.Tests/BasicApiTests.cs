using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace RareAPI.Tests;

public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BasicApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Configure test environment
            builder.UseEnvironment("Testing");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Welcome to Rare API!", content);
    }

    [Fact]
    public async Task GetNonExistentEndpoint_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/")]
    public async Task KnownEndpoints_DoNotReturnInternalServerError(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // We don't expect 500 Internal Server Error for these endpoints
        // They might return 401, 404, or other status codes, but not 500
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}