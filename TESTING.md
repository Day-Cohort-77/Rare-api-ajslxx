# Testing Setup and Automation

This document describes the automated testing setup for the Rare API project.

## Overview

The project uses automated xUnit testing with GitHub Actions to ensure code quality and prevent integration issues. All pull requests must pass tests before merging into the main branch.

## Test Structure

- **Test Project**: `RareAPI.Tests/` 
- **Test Framework**: xUnit with ASP.NET Core Testing
- **Test Types**: Integration tests using `WebApplicationFactory`

## Running Tests Locally

### Prerequisites
- .NET 8.0 SDK installed
- Project dependencies restored

### Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests for specific project
dotnet test RareAPI.Tests/RareAPI.Tests.csproj
```

## CI/CD Workflow

### Automated Testing
The GitHub Actions workflow (`.github/workflows/run-tests.yml`) automatically:

1. **Triggers on**:
   - Pull request creation/updates to `main` branch
   - Direct pushes to `main` branch

2. **Test Process**:
   - Sets up .NET 8.0 environment
   - Restores project dependencies
   - Builds the solution in Release configuration
   - Runs all xUnit tests
   - Collects code coverage data
   - Uploads test results as artifacts

3. **Artifacts**:
   - Test results are stored for 7 days
   - Code coverage reports are uploaded to Codecov (if configured)

### Branch Protection

To enforce testing requirements:

1. Go to repository Settings → Branches
2. Add branch protection rule for `main`
3. Enable "Require status checks to pass before merging"
4. Select "test" as a required status check
5. Enable "Require pull request reviews before merging" (recommended)

## Writing Tests

### Basic Test Structure

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace RareAPI.Tests;

public class YourTestClass : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public YourTestClass(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task YourTest_ShouldWork()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/your-endpoint");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

### Test Environment Configuration

The application automatically skips database initialization when running in "Testing" environment to avoid dependency issues during testing.

## Troubleshooting

### Common Issues

1. **Database Connection Errors**: 
   - Ensure tests use "Testing" environment
   - Database services are automatically skipped in test environment

2. **Build Failures**:
   - Check that all dependencies are restored
   - Verify .NET 8.0 SDK is installed

3. **Test Discovery Issues**:
   - Ensure test classes are public
   - Test methods should be marked with `[Fact]` or `[Theory]`
   - Test project should reference `xunit` packages

### Viewing Test Results

- **Local**: Test results appear in console output
- **GitHub**: Check the Actions tab for detailed test results
- **PR Reviews**: Test status shows in PR checks section

## Adding New Tests

1. Create test classes in `RareAPI.Tests/` directory
2. Follow naming convention: `[FeatureName]Tests.cs`
3. Use `WebApplicationFactory<Program>` for integration tests
4. Test critical functionality and edge cases
5. Ensure tests are isolated and don't depend on external services

## Maintenance

- Review and update tests when API changes
- Monitor test performance and optimize slow tests
- Keep test dependencies up to date
- Review code coverage reports regularly