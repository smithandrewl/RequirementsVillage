# Unit Testing Tasks

## Overview
Requirements Village already has a comprehensive unit testing setup in place for both backend and frontend. This document outlines the current testing infrastructure.

## Current Testing Setup

### Backend Testing (Already Implemented)
- **Framework**: xUnit with FsUnit for assertions
- **Property-based testing**: FsCheck for generative testing
- **Data generation**: Bogus for realistic test data
- **Coverage**: Integrated with `dotnet test --collect:"XPlat Code Coverage"`
- **Location**: `RequirementsVillage.Api.Tests/`

### Frontend Testing (Already Implemented)
- **Framework**: Fable.Mocha for test runner
- **Test structure**: Organized by feature (State, Components, API)
- **Coverage**: NYC for JavaScript code coverage
- **Location**: `RequirementsVillage.Client/tests/`

### Shared Test Infrastructure
- **Test generators**: Shared between backend and frontend in `TestGenerators.fs`
- **Realistic data**: Bogus generates company names, descriptions, dates
- **Property tests**: FsCheck generates domain types and edge cases

## Completed Tasks

### ✅ Backend Test Infrastructure
- [x] xUnit test framework configured
- [x] FsCheck for property-based testing
- [x] Bogus for realistic data generation
- [x] Integration tests with TestServer
- [x] Unit tests for services and repositories
- [x] Property tests for domain invariants
- [x] Test data generators in TestGenerators.fs

### ✅ Frontend Test Infrastructure
- [x] Fable.Mocha test runner configured
- [x] Test script in package.json (`npm test`)
- [x] NYC coverage reporting configured
- [x] Test folder structure organized by feature
- [x] Shared test helpers and data generators

### ✅ Implemented Test Examples
- [x] Project validation property tests
- [x] Serialization round-trip tests
- [x] State update function tests
- [x] Component rendering tests
- [x] API client tests with mocked responses
- [x] Integration tests for all endpoints

## Running Tests

### Backend Tests
```bash
cd RequirementsVillage.Api.Tests
dotnet test                                    # Run all tests
dotnet test --collect:"XPlat Code Coverage"   # With coverage
dotnet test --filter "FullyQualifiedName~PropertyTests"  # Filter tests
```

### Frontend Tests
```bash
cd RequirementsVillage.Client
npm test              # Run tests in watch mode
npm run test:once     # Run once and exit
npm run test:coverage # Run with coverage report
```

## Test Data Generation Examples

### Current Implementation (TestGenerators.fs)
```fsharp
// Bogus for realistic data
let projectFaker = 
  Faker<Project>()
    .RuleFor(fun p -> p.Name, fun f -> f.Company.CatchPhrase())
    .RuleFor(fun p -> p.Description, fun f -> f.Lorem.Paragraph())

// FsCheck for property testing
let projectGen = 
  gen {
    let! name = Gen.nonEmptyString
    let! status = Arb.generate<ProjectStatus>
    return { Name = name; Status = status; ... }
  }
```

## Best Practices (Already Followed)
1. **Test organization**: Tests mirror source structure
2. **Shared generators**: Reusable test data between suites
3. **Property tests**: Test invariants, not just examples
4. **Realistic data**: Use Bogus for human-readable test output
5. **Fast feedback**: Tests run quickly for TDD workflow
6. **Coverage goals**: Aim for >80% coverage (configured in package.json)

## Notes
- The testing infrastructure is already comprehensive
- Both unit and integration tests are in place
- Property-based testing with FsCheck is implemented
- Realistic data generation with Bogus is configured
- No need for additional test runners or frameworks