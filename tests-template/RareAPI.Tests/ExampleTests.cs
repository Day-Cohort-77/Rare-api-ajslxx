using Microsoft.AspNetCore.Mvc.Testing;

namespace RareAPI.Tests;

/// <summary>
/// Example unit tests for the RareAPI project.
/// This demonstrates the basic test setup and structure.
/// </summary>
public class ExampleTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = "Welcome to Rare API!";
        
        // Act
        var actual = "Welcome to Rare API!";
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Math_Addition_ShouldReturnCorrectSum()
    {
        // Arrange
        int a = 5;
        int b = 3;
        int expected = 8;
        
        // Act
        int result = a + b;
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("test", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_HasValue_ShouldReturnExpectedResult(string input, bool expected)
    {
        // Act
        bool result = !string.IsNullOrEmpty(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
}

/// <summary>
/// Example integration tests using WebApplicationFactory.
/// This demonstrates how to test the API endpoints.
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Get_Homepage_ReturnsWelcomeMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Welcome to Rare API!", content);
    }
}