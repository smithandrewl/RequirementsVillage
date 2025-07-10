# Requirements Village Client Tests

Comprehensive unit tests for the Requirements Village F# Fable client application, following Test-Driven Development (TDD) principles.

## Test Structure

```
tests/
├── Helpers/              # Test utilities and helpers
│   ├── TestData.fs      # Test data generators using Bogus
│   ├── TestHelpers.fs   # Assertion helpers and test utilities
│   └── ApiMock.fs       # API mocking utilities
├── State/               # State management tests
│   ├── TypesTests.fs    # Tests for state types and helper functions
│   ├── UpdateTests.fs   # Tests for update function and message handlers
│   ├── StatePropertyTests.fs  # Property-based test patterns
│   └── CommandTests.fs  # Tests for Elmish command generation
├── Components/          # UI component tests
│   ├── ProjectCardTests.fs
│   └── ThemeSelectorTests.fs
├── Api/                 # API integration tests
│   ├── ProjectApiTests.fs
│   └── CodecsTests.fs
└── Main.fs             # Test runner entry point
```

## Running Tests

### One-time test run
```bash
npm run test:once
```

### Watch mode (re-runs on file changes)
```bash
npm test
# or
npm run test:watch
```

### Build tests only (without running)
```bash
dotnet fable tests
```

## Test Coverage

### State Management Tests

#### TypesTests.fs
- **UIState helper functions**: Tests for `isLoading`, `isLoadingAny`, `isLoadingProject`
- **Model initialization**: Verifies default values
- **State builder functions**: Tests for `withProjects`, `withPage`, `withError`, etc.
- **LoadingOperation union**: Tests equality and Set operations
- **Edge cases**: Empty states, multiple operations

#### UpdateTests.fs
- **init function**: Verifies initial state and commands
- **Message handlers**: Tests for all Msg types
  - `NavigateTo`: Page navigation with conditional loading
  - `SetTheme`: Theme switching
  - `LoadProjects`: Async loading initiation
  - `ProjectsLoaded`: Success and error handling
  - `FilterByStatus`: Project filtering
  - `StartLoading/StopLoading`: Loading state management
  - `ClearError`: Error clearing
- **State transitions**: Full workflow testing
- **Edge cases**: Error recovery, concurrent operations

#### StatePropertyTests.fs
- **State invariants**: Ensures consistency across operations
- **Message ordering**: Tests idempotency and commutativity
- **Complex scenarios**: Rapid changes, concurrent operations
- **Error recovery workflows**: Complete error handling paths

#### CommandTests.fs
- **Command generation**: Verifies correct Elmish commands
- **Batch commands**: Tests command composition
- **Pure vs effectful**: Ensures pure messages don't generate commands
- **Command extraction**: Tests helper functions

## Test Patterns

### 1. Arrange-Act-Assert
```fsharp
test "example test" {
  // Arrange
  let model = State.initialModel()
  
  // Act
  let newModel, cmd = update SomeMessage model
  
  // Assert
  Assert.equal expectedValue newModel.SomeField "Should update correctly"
}
```

### 2. Property-Based Testing Patterns
```fsharp
test "property test" {
  for _ in 1..100 do
    let randomInput = generateRandom()
    let result = functionUnderTest randomInput
    Assert.isTrue (invariantHolds result) "Invariant should hold"
}
```

### 3. Test Data Generation
```fsharp
// Using Bogus for realistic test data
let projects = Generate.projects 5
let projectWithStatus = Generate.projectWithStatus InProgress
```

## Writing New Tests

### Guidelines
1. **Test behavior, not implementation**: Focus on what the code does, not how
2. **Use descriptive test names**: Clearly state what is being tested
3. **Keep tests independent**: Each test should be runnable in isolation
4. **Test edge cases**: Empty lists, null values, error conditions
5. **Use test helpers**: Leverage existing helpers for consistency

### Example Test Module
```fsharp
module MyFeatureTests

open Fable.Mocha
open RequirementsVillage.Client.Tests.Helpers.TestHelpers

let tests =
  testList "MyFeature Tests" [
    
    testList "Feature Category" [
      
      test "specific behavior" {
        // Test implementation
        Assert.isTrue true "Should pass"
      }
    ]
  ]
```

## Test Dependencies

- **Fable.Mocha**: Test framework for F# in JavaScript
- **Bogus**: Realistic test data generation
- **Mocha**: JavaScript test runner
- **jsdom**: DOM simulation for browser APIs

## Continuous Integration

Tests are designed to run in CI environments without requiring a browser:
- Uses jsdom for DOM APIs
- No external dependencies required
- All tests run in Node.js environment

## Debugging Tests

1. Add `console.log` statements in test code
2. Use browser debugger with `debugger` keyword
3. Run specific test suites by modifying Main.fs temporarily
4. Check test output for detailed error messages

## Known Limitations

- Component rendering tests use placeholder implementations
- Browser-specific APIs are mocked (localStorage, etc.)
- Async operations use simplified promise handling