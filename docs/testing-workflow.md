# GitHub Actions Testing Workflow Documentation

## Overview

This repository includes an automated testing workflow that runs on all pull requests and pushes to the main branch. The workflow ensures code quality by building the project and running all available xUnit tests before allowing merges.

## Workflow Configuration

The GitHub Actions workflow is located at `.github/workflows/run-tests.yml` and includes:

- **Automatic Triggers**: Runs on push to main and all pull requests targeting main
- **Build Validation**: Compiles the project in Release configuration
- **Test Execution**: Runs all xUnit tests (when available)
- **Test Reporting**: Uploads test results as artifacts for debugging

## Current Status

✅ **Workflow Active**: Automatically runs on PR creation and updates  
✅ **Build Validation**: Ensures code compiles successfully  
⏳ **Test Setup**: Ready for xUnit test projects to be added  

## Adding Tests

To add xUnit tests to this project:

1. Create a test project:
   ```bash
   mkdir tests
   cd tests
   dotnet new xunit -n RareAPI.Tests
   ```

2. Add the test project to the solution:
   ```bash
   dotnet sln add tests/RareAPI.Tests/RareAPI.Tests.csproj
   ```

3. Add a reference to the main project:
   ```bash
   cd tests/RareAPI.Tests
   dotnet add reference ../../RareAPI.csproj
   ```

4. Write your tests in the test project and commit - the workflow will automatically run them!

## Branch Protection

To enforce test requirements before merging:

1. Go to repository Settings → Branches
2. Add a branch protection rule for `main`
3. Enable "Require status checks to pass before merging"
4. Select "Run Tests" as a required check
5. Enable "Require branches to be up to date before merging"

## Troubleshooting

### Build Failures
- Check the workflow logs in the Actions tab
- Ensure all dependencies are properly restored
- Verify project references are correct

### Test Failures
- Review test results in the workflow artifacts
- Check individual test output in the logs
- Ensure all required services and configurations are available in the test environment

### Workflow Not Running
- Verify the workflow file is in `.github/workflows/`
- Check that the file is properly formatted YAML
- Ensure branch names match the triggers in the workflow

## Success Metrics

- ✅ Zero failed merges due to build issues
- ✅ Clear visibility of build status in PR interface
- 🎯 **Next**: Add comprehensive test coverage
- 🎯 **Next**: Set up branch protection rules

## Workflow Details

The workflow runs the following steps:
1. **Checkout**: Gets the latest code
2. **Setup .NET**: Installs .NET 8.0 SDK
3. **Restore**: Downloads all NuGet dependencies
4. **Build**: Compiles in Release configuration
5. **Test**: Runs all xUnit tests and generates reports
6. **Upload Results**: Saves test results as artifacts

The workflow is designed to:
- ✅ Pass when build succeeds (even with no tests)
- ❌ Fail when build fails
- ❌ Fail when tests fail (once tests are added)
- 📊 Provide clear feedback on all outcomes