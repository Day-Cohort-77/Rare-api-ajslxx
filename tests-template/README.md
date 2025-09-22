# Test Template Setup Instructions

## Quick Start

To set up testing for this project, follow these steps:

1. **Copy the template to the main directory:**
   ```bash
   cp -r tests-template tests
   ```

2. **Add the test project to the solution:**
   ```bash
   dotnet sln add tests/RareAPI.Tests/RareAPI.Tests.csproj
   ```

3. **Run the tests to verify setup:**
   ```bash
   dotnet test
   ```

4. **Delete this template directory (optional):**
   ```bash
   rm -rf tests-template
   ```

## What's Included

- ✅ **Basic xUnit project** with proper configuration
- ✅ **Example unit tests** showing test structure
- ✅ **Integration test examples** using WebApplicationFactory
- ✅ **Proper project references** to the main API
- ✅ **Test packages** for comprehensive testing

## Test Structure

```
tests/
└── RareAPI.Tests/
    ├── RareAPI.Tests.csproj    # Test project configuration
    ├── GlobalUsings.cs         # Global using statements
    └── ExampleTests.cs         # Example test classes
```

## Adding Your Own Tests

1. Create new test classes in the `tests/RareAPI.Tests/` directory
2. Follow the naming convention: `[FeatureName]Tests.cs`
3. Use the `[Fact]` attribute for simple tests
4. Use the `[Theory]` attribute with `[InlineData]` for parameterized tests
5. Use `WebApplicationFactory<Program>` for integration tests

## Running Tests

- **All tests:** `dotnet test`
- **Specific project:** `dotnet test tests/RareAPI.Tests/`
- **With verbose output:** `dotnet test --verbosity normal`
- **Generate coverage:** `dotnet test --collect:"XPlat Code Coverage"`

The GitHub Actions workflow will automatically run these tests on every pull request!