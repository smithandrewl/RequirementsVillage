# RequirementsVillage.Api.Tests

Comprehensive test suite for the Requirements Village API, following Test-Driven Development (TDD) principles.

## Test Structure

```
RequirementsVillage.Api.Tests/
├── Helpers/                 # Test utilities and helpers
│   ├── TestHelpers.fs      # Common test helpers and assertions
│   ├── Generators.fs       # Test data generators (Bogus & FsCheck)
│   └── Fixtures.fs         # Test fixtures and mocks
├── Unit/                   # Unit tests
│   ├── Models/            # Model tests
│   ├── Services/          # Service layer tests
│   └── Repository/        # Repository tests
└── Integration/           # Integration tests
    └── API endpoint tests
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Dependencies

- **xUnit**: Main testing framework
- **FsUnit**: F# assertion library
- **FsCheck**: Property-based testing
- **Bogus**: Realistic test data generation
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **NSubstitute**: Mocking framework

## Test Patterns

### Unit Tests
- Test individual components in isolation
- Use mocks for dependencies
- Focus on business logic validation
- Property-based tests for complex scenarios

### Integration Tests
- Test API endpoints end-to-end
- Use TestWebApplicationFactory
- Verify HTTP status codes and responses
- Test error handling scenarios

## TDD Workflow

1. Write failing test first
2. Implement minimum code to pass
3. Refactor while keeping tests green
4. Repeat for each feature