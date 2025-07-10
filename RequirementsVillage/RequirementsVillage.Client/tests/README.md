# Requirements Village Client Tests

This directory contains the test suite for the Requirements Village F# Fable/Elmish frontend application.

## Test Framework

We use **Fable.Mocha** as our testing framework, which provides:
- Integration with the Mocha test runner
- Support for async tests with promises
- Works well with Elmish applications
- Can run tests in both Node.js and browser environments

## Test Structure

```
tests/
├── State/           # Elmish state management tests
│   ├── TypesTests.fs
│   └── UpdateTests.fs
├── Components/      # UI component tests
│   ├── ProjectCardTests.fs
│   └── ThemeSelectorTests.fs
├── Api/            # API client tests
│   ├── ProjectApiTests.fs
│   └── CodecsTests.fs
├── Helpers/        # Test utilities and helpers
│   ├── TestData.fs      # Test data generators using Bogus
│   ├── TestHelpers.fs   # Assert helpers and test utilities
│   └── ApiMock.fs       # API mocking utilities
├── Main.fs         # Test runner entry point
├── setup.js        # JavaScript test environment setup
└── README.md       # This file
```

## Running Tests

```bash
# Run tests once
npm run test:once

# Run tests in watch mode (re-runs on file changes)
npm run test:watch

# Run tests with Fable watch mode
npm test
```

## Writing Tests

### Basic Test Structure

```fsharp
module MyModuleTests

open Fable.Mocha
open RequirementsVillage.Client.Tests.Helpers.TestHelpers

let tests =
  testList "My Module Tests" [
    
    test "should do something" {
      let result = myFunction()
      Assert.equal expected result "Result should match expected"
    }
    
    testAsync "should handle async operations" {
      let! result = myAsyncFunction()
      Assert.isTrue (result > 0) "Result should be positive"
    }
  ]
```

### Using Test Helpers

The test helpers provide utilities for:

1. **Test Data Generation** (TestData.fs):
   - `Generate.project()` - Creates random project data
   - `Generate.projects count` - Creates multiple projects
   - `Sample.*` - Pre-defined test data

2. **Assertions** (TestHelpers.fs):
   - `Assert.equal` - Value equality
   - `Assert.isTrue/isFalse` - Boolean assertions
   - `Assert.contains` - Collection membership
   - `Assert.isNone/isSome` - Option assertions

3. **State Testing** (TestHelpers.fs):
   - `State.initialModel()` - Create initial model
   - `State.withProjects` - Builder functions for model
   - `Elmish.extractMessages` - Extract messages from commands

4. **API Mocking** (ApiMock.fs):
   - `MockApiClient` - Mock API responses
   - `Scenarios.*` - Pre-configured API scenarios
   - `FetchMock.withMock` - Replace fetch for testing

### Test Categories

1. **State Tests**: Test Elmish update functions and state transitions
2. **Component Tests**: Test UI component rendering and behavior
3. **API Tests**: Test API client functions and error handling
4. **Codec Tests**: Test JSON encoding/decoding

## Best Practices

1. **Follow TDD**: Write tests before implementing features
2. **Test Names**: Use descriptive test names that explain what is being tested
3. **Isolation**: Each test should be independent and not rely on others
4. **Mocking**: Use mocks for external dependencies (API calls, storage)
5. **Coverage**: Aim for high test coverage, especially for business logic

## Dependencies

- `Fable.Mocha`: F# testing framework
- `Bogus`: Realistic test data generation
- `mocha`: JavaScript test runner
- `jsdom`: DOM simulation for Node.js
- `jsdom-global`: Global jsdom setup

## Troubleshooting

- If tests fail to compile, ensure all F# files are included in the test project file
- For "module not found" errors, run `npm install` in the client directory
- For async test timeouts, increase the timeout in `.mocharc.json`