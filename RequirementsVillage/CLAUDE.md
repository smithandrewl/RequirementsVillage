# CLAUDE.md

## 🏗️ Project Overview

**Project Name:** Requirements Village  
**Tagline:** *Where project ideas get laid to rest*  
**Purpose:** An application designed to help users catalog, organize, and reflect on their myriad project ideas—especially those that may never come to fruition. It serves as a structured repository for the indecisive, allowing for detailed specification without the commitment of execution.

---

## 🧰 Tech Stack

- **Frontend:** F# + Fable + Elmish + Feliz + Feliz.Bulma + Bulma CSS
- **Backend:** F# + Giraffe + ASP.NET Core
- **Data Access:** Dapper with SQLite (implemented but using in-memory storage)
- **Authentication:** ASP.NET Core Identity (not yet implemented)
- **Database:** SQLite (code ready, not yet connected)

---

## 📂 Project Structure

```
RequirementsVillage/
├── RequirementsVillage.Api/     # F# Giraffe API backend
│   ├── Program.fs                      # Main entry point
│   ├── Endpoints.fs                    # API route handlers
│   ├── Models.fs                       # Domain models
│   ├── Database.fs                     # Repository implementations
│   ├── Services.fs                     # Business logic
│   └── wwwroot/                        # Static client files (built)
├── RequirementsVillage.Client/  # F# Fable frontend (SPA)
│   ├── src/
│   │   ├── App.fs                      # Main app entry
│   │   ├── State.fs                    # Elmish state management
│   │   ├── Types.fs                    # Shared types
│   │   ├── Api/                        # API client modules
│   │   ├── Components/                 # UI components
│   │   └── Pages/                      # Page components
│   ├── webpack.config.js               # Webpack configuration
│   └── package.json                    # Node dependencies
└── RequirementsVillage.sln             # .NET solution file
```

---

## 🚀 Current Implementation Status

### ✅ Implemented Features

#### Backend
- Full CRUD API for projects (GET, POST, PUT, PATCH, DELETE)
- Health check endpoint
- Repository pattern with interface
- In-memory data storage (with sample projects)
- SQLite/Dapper repository (implemented but not connected)
- CORS configuration for development
- Static file serving for SPA
- F# discriminated unions for error handling
- JSON serialization with F# type support

#### Frontend
- Landing page with gothic theme
- Dashboard with project cards
- Theme selector (light/dark mode)
- API integration for fetching projects
- Elmish Model-View-Update architecture
- Type-safe HTML with Feliz
- Webpack build with hot reload
- Bulma CSS + Feliz.Bulma type-safe styling

### ❌ Not Yet Implemented

- User authentication and authorization
- Database persistence (SQLite connection)
- User registration and login
- Project ownership and access control
- Tags and advanced filtering
- Search functionality
- Export functionality
- Test projects

---

## 🛠️ Development Commands

### Full Development (both API and Client)
```bash
# Terminal 1: Run the API
cd RequirementsVillage/RequirementsVillage.Api
dotnet run

# Terminal 2: Run client with hot reload
cd RequirementsVillage/RequirementsVillage.Client
npm install
npm start
```

- API: http://localhost:5000
- Client dev server: http://localhost:8080

### Production Build
```bash
# Build client
cd RequirementsVillage/RequirementsVillage.Client
npm run build

# Run API (serves both API and static files)
cd RequirementsVillage/RequirementsVillage.Api
dotnet run --configuration Release
```

### Testing Commands

#### Backend Tests
```bash
# Run all backend tests
cd RequirementsVillage.Api.Tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run with filter
dotnet test --filter "FullyQualifiedName~ProjectService"
```

#### Frontend Tests
```bash
# Run all frontend tests
cd RequirementsVillage.Client
npm test

# Run tests once (no watch)
npm run test:once

# Run with coverage
npm run test:coverage

# Generate coverage report
npm run coverage:report
```


### Coverage Reports
After running tests with coverage, reports are generated in:
- `./coverage/backend/` - Backend coverage reports
- `./coverage/frontend/` - Frontend coverage reports
- `./coverage/summary.txt` - Coverage summary

HTML reports can be viewed by opening:
- Backend: `./coverage/backend/index.html`
- Frontend: `./coverage/frontend/index.html`

---

## 🌐 API Endpoints

### Projects
- `GET /api/projects` - Get all projects
- `GET /api/projects/{id}` - Get specific project
- `POST /api/projects` - Create new project
- `PUT /api/projects/{id}` - Update project
- `PATCH /api/projects/{id}/status` - Update project status
- `DELETE /api/projects/{id}` - Delete project

### System
- `GET /api/health` - Health check with timestamp

---

## 🧭 Planned Features

- **User Management:**
  - Account registration and login
  - Password reset functionality
  - User profiles

- **Enhanced Project Management:**
  - Project ownership
  - Tags and categories
  - Advanced search and filtering
  - Project templates
  - Export to various formats

- **Technical Improvements:**
  - Switch from in-memory to SQLite persistence
  - Add comprehensive test coverage
  - Implement Fable.Remoting for type-safe RPC
  - Add application logging
  - Deploy to production hosting

## 🎨 Visual References

These mockups demonstrate the intended tone, layout, and visual style of Requirements Village. All images are stored in `./assets/claude/`.

---

### 🧩 UI Mockup

A professional, SaaS-style interface mockup showing the project list screen layout and component arrangement:

![UI Mockup](./assets/claude/ui-mockup.png)

---

### 🪦 Landing Page Mockup

Full-screen Gothic landing page concept featuring a project burial scene with overlaid branding. This sets the tone of polished irreverence and light existential despair:

![Landing Page Mockup](./assets/claude/landing-page-mockup.png)

---

### 🗿 Icon Mockup

A realistic app icon concept showing a gravestone with a project folder symbol — suitable for branding, headers, or favicon use:

![Icon Mockup](./assets/claude/icon-mockup.png)

---

## 🏗️ Architecture Notes

### Frontend Architecture
- **Elmish**: Model-View-Update pattern for predictable state management
- **Feliz**: Type-safe React bindings and HTML DSL
- **Pattern Matching**: Exhaustive handling of all application states
- **Async Commands**: Side effects handled through Elmish commands

### Backend Architecture
- **Giraffe**: Functional HTTP handlers composed with fish operators (>=>)
- **Repository Pattern**: Clean separation between data access and business logic
- **Result Types**: All operations return Result<'Success, 'Error>
- **Dependency Injection**: Services registered in ASP.NET Core DI container

### Code Style
- **Indentation**: 2 spaces for all F# and JavaScript files
- **Line Length**: 80 characters maximum (hard wrap)
- **F# Conventions**: 
  - Prefer immutable data structures
  - Use pattern matching over if/else
  - Leverage type inference
  - Organize code with modules

### Development Methodology
- **Test-Driven Development (TDD)**: 
  - Write tests BEFORE implementing features
  - Follow Red-Green-Refactor cycle
  - Start with failing tests that define behavior
  - Implement minimum code to make tests pass
  - Refactor while keeping tests green
  - Use property-based tests with FsCheck for complex logic
  - Use Faker/Bogus for realistic test data generation
  - Aim for high test coverage from the start
- **No Scripts Policy**:
  - Avoid creating scripts (.fsx, .py, .sh, .cmd, .bat) whenever possible
  - Use built-in tooling (dotnet CLI, npm scripts) instead
  - Keep build and development processes simple and standard

### Formatting Preferences

#### Record Types
Align field names and types with spacing:
```fsharp
type Project = {
  Id:          Guid
  Name:        string
  Description: string
  Category:    ProjectCategory
  Status:      ProjectStatus
  CreatedAt:   DateTime
  UpdatedAt:   DateTime
}
```

#### Discriminated Unions
Simple cases on single lines, complex cases with aligned fields:
```fsharp
type ProjectError =
  | NotFound of
      projectId:     Guid
    * searchContext: string
  | ValidationFailed of
      field:         string
    * reason:        string
    * attemptedValue: obj
  | UnknownError of message: string
```

#### SQL Queries
Use triple-quoted strings with aligned columns and keywords:
```fsharp
let selectAll = """
  SELECT
    Id,
    Name,
    Description,
    Category,
    Status,
    CreatedAt,
    UpdatedAt
  FROM
    Projects
  ORDER BY
    UpdatedAt DESC
"""

let update = """
  UPDATE
    Projects
  SET
    Name        = @Name,
    Description = @Description,
    Category    = @Category,
    Status      = @Status,
    UpdatedAt   = @UpdatedAt
  WHERE
    Id = @Id
"""
```

#### Pattern Matching
Align match cases and arrows with consistent spacing:
```fsharp
match status with
| Idea       -> "idea"
| InProgress -> "inProgress"
| Completed  -> "completed"
| Abandoned  -> "abandoned"
| OnHold     -> "onHold"
```

#### Function Body Formatting
Group statements by type, align equals signs within each group, reset alignment for new groups:
```fsharp
let processProject project =
  let name     = project.Name
  let category = project.Category
  
  let result         = validateProject project
  let updatedProject = updateTimestamp project
  
  saveProject updatedProject
  logActivity project.Id
```

#### Function Parameters
For multiple parameters, align on separate lines:
```fsharp
member _.CreateProjectAsync(
  name:        string,
  description: string,
  category:    ProjectCategory
) =
```

### Current Configuration
- **Data Storage**: In-memory repository with sample data
- **Frontend Build**: Webpack outputs to API's wwwroot folder
- **CORS**: Enabled for localhost:8080 in development
- **JSON**: Custom F# converters for discriminated unions

### Testing Infrastructure
Requirements Village is a full-stack F# application with comprehensive test coverage:
- **Backend Tests**: xUnit, FsUnit, FsCheck, Bogus for API and business logic
- **Frontend Tests**: Fable.Mocha for Elmish state and components
- **Test Runner**: Standard dotnet test for backend, npm test for frontend
- **Coverage**: Integrated coverage reporting for both backend and frontend

### Complete Feature Testing Example: Create Project

This project demonstrates comprehensive testing for the "Create Project" feature across all layers:

#### 1. Backend Unit Tests (ProjectServiceTests.fs)
- Tests business logic for creating projects
- Validates that new projects start with "Idea" status
- Ensures timestamps are set correctly
- Tests validation rules (empty names, long descriptions)

#### 2. Backend Integration Tests (ProjectEndpointsTests.fs)
- Tests the full HTTP request/response cycle
- Validates HTTP status codes (201 Created)
- Checks response headers (Location)
- Tests error scenarios (400 Bad Request)

#### 3. Repository Tests (InMemoryProjectRepositoryTests.fs)
- Tests data persistence layer
- Validates CRUD operations
- Tests concurrent access scenarios

#### 4. Property-Based Tests (PropertyTests.fs)
- Tests with randomly generated valid/invalid data
- Ensures invariants hold across all inputs
- Tests edge cases automatically

#### 5. Frontend State Tests (UpdateTests.fs)
- Tests Elmish update functions
- Validates state transitions
- Tests command generation

#### 6. Frontend Component Tests (ProjectCardTests.fs)
- Tests UI component rendering
- Validates user interactions
- Tests component props

#### 7. Frontend API Tests (ProjectApiTests.fs)
- Tests API client functions
- Validates request/response encoding
- Tests error handling

#### Test Data Generation
- Shared test data generators in TestGenerators.fs
- Bogus for realistic test data
- FsCheck generators for property tests
- Consistent test data across all test suites

### Running Tests
```bash
# Backend tests
cd RequirementsVillage.Api.Tests
dotnet test

# Frontend tests
cd RequirementsVillage.Client
npm test
```

### Test Organization
- **Backend**: RequirementsVillage.Api.Tests/
  - Unit tests for Models, Services, Repository
  - Integration tests for API endpoints
  - Property-based tests with FsCheck
- **Frontend**: RequirementsVillage.Client/tests/
  - State management tests
  - Component tests
  - API client tests