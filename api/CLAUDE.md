# CLAUDE.md - API

## 🎯 Project Context

This is the backend API for Requirements Village, built as a .NET 8 Minimal API. Currently a simple hello-world implementation with plans for functional programming patterns.

## 🧰 Tech Stack

- **Framework:** .NET 8 Minimal API
- **Language:** C# (functional style planned)
- **Database:** SQLite (planned)
- **ORM:** Entity Framework Core (planned)
- **Authentication:** ASP.NET Core Identity (planned)

## 📁 Key Files

```
api/RequirementsVillage.Api/
├── Program.cs                    # Main entry point with endpoints
└── RequirementsVillage.Api.csproj # Project file
```

## 🛠️ Development Commands

```bash
cd api/RequirementsVillage.Api
dotnet restore              # Restore packages
dotnet run                  # Start development server
dotnet build                # Build project
```

## 🌐 Current Endpoints

- `GET /` - API identification
- `GET /api/health` - Health check with timestamp

## 🏗️ Architecture Overview

This API follows a **layered architecture** with clean separation of concerns and dependency injection throughout. The architecture emphasizes business rule enforcement, type safety through Result types, and functional programming patterns.

### 🎯 Core Principles

- **Business Logic Protection:** Domain services enforce business rules and prevent misuse of the persistence layer
- **Railway Oriented Programming (ROP):** Use discriminated unions and Result types for predictable error handling
- **Immutable Data:** Models are immutable records where possible
- **Dependency Injection:** All services injected via interfaces for testability and flexibility
- **Single Responsibility:** Each layer has a focused purpose

### 📁 Project Structure

```
Api/
├── Program.cs              # Main entry point + DI setup
├── Endpoints/              # Endpoint groups (ProjectEndpoints.cs, etc.)
├── Extensions/             # Service registration extensions
├── Middleware/             # Custom middleware (if needed)
└── Api.csproj

Domain/
├── Interfaces/
│   └── IProjectService.cs  # Domain service interfaces
├── Services/
│   └── ProjectService.cs   # Business logic services
└── Domain.csproj

Models/
├── Project.cs              # Immutable records (shared by API and EF)
├── Category.cs
├── User.cs
└── Models.csproj

Persistence/
├── Interfaces/
│   └── IProjectRepository.cs  # Repository interfaces
├── Repositories/
│   └── ProjectRepository.cs   # CRUD implementations
├── Data/                      # DbContext and configurations
└── Persistence.csproj

Tests/
└── Tests.csproj

RequirementsVillage.sln
```

### 🔄 Data Flow

```
API Endpoints → Domain Services → Repository Interfaces → Repository Implementations → Database
     ↑              ↑                     ↑                        ↑
   Minimal API   Business Rules    Interface Contracts        CRUD Operations
```

### 🎭 Layer Responsibilities

#### **Api Layer**
- **Purpose:** HTTP endpoints and request/response handling
- **Contains:** Minimal API endpoints organized by feature
- **Dependencies:** Domain service interfaces only
- **Pattern:** Endpoint handlers pattern match on Result types from Domain services

#### **Domain Layer** 
- **Purpose:** Business logic and rule enforcement
- **Contains:** Services that validate business rules and coordinate data operations
- **Dependencies:** Models + Persistence interfaces
- **Error Handling:** Returns Result<TSuccess, TError> types using Railway Oriented Programming
- **Rule Enforcement:** Prevents misuse of persistence layer through validation

#### **Models Layer**
- **Purpose:** Shared data structures
- **Contains:** Immutable records used by both API serialization and Entity Framework
- **Dependencies:** None (pure data)
- **Note:** Single model set (no DTOs unless specifically needed)

#### **Persistence Layer**
- **Purpose:** Data access and CRUD operations
- **Contains:** Repository implementations, DbContext, and EF configurations
- **Dependencies:** Models only
- **Pattern:** Simple CRUD operations with no business logic

### 🏷️ Naming Conventions

#### **Services & Interfaces**
- **Domain Services:** `ProjectService`, `CategoryService`, `UserService`
- **Domain Interfaces:** `IProjectService`, `ICategoryService`, `IUserService`  
- **Repositories:** `ProjectRepository`, `CategoryRepository`, `UserRepository`
- **Repository Interfaces:** `IProjectRepository`, `ICategoryRepository`, `IUserRepository`

#### **Models**
- **Models:** `Project`, `Category`, `User` (used for both API and database)
- **Result Types:** `Result<Project, Error>`, `Result<List<Project>, Error>`

#### **Endpoints**
- **Endpoint Classes:** `ProjectEndpoints`, `CategoryEndpoints`, `UserEndpoints`
- **Methods:** `MapProjectEndpoints()`, `GetAllProjects()`, `CreateProject()`

### 🚂 Railway Oriented Programming

#### **Why Railway Oriented Programming**
1. **Compile-time Safety:** Forgetting to handle errors causes compilation errors
2. **Explicit Contracts:** Method signatures show what can fail
3. **Type System Integration:** Failures are part of the type system
4. **Composable:** Chain operations without nested error checking
5. **Predictable Flow:** Success and failure paths are explicit

#### **Usage Pattern**
```csharp
// Domain Service
public async Task<Result<Project, Error>> GetProjectAsync(int id)
{
    var project = await _repository.GetByIdAsync(id);
    return project is null 
        ? Error.NotFound($"Project {id} not found")
        : Result.Success(project);
}

// API Endpoint
app.MapGet("/api/projects/{id}", async (int id, IProjectService service) => 
{
    var result = await service.GetProjectAsync(id);
    return result.IsSuccess 
        ? Results.Ok(result.Value)
        : Results.NotFound(result.Error.Message);
});
```

### 🔌 Dependency Injection Setup

#### **Service Lifetimes**
- **Domain Services:** Scoped (per request)
- **Repositories:** Scoped (per request)  
- **DbContext:** Scoped (per request)

#### **Registration Pattern**
```csharp
// In Program.cs or Extensions
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
```

## 📝 Development Notes

### **Current State**
- CORS configured for frontend development (localhost:5173)
- Basic hello-world endpoints implemented
- Ready for architectural implementation

### **Implementation Strategy**
- Start with single project, refactor to multi-project structure
- Implement Result types early for consistent error handling
- Add business services incrementally with proper validation
- Database and authentication features planned for later commits

### **Key Dependencies (Planned)**
- Entity Framework Core for data access
- ASP.NET Core Identity for authentication
- Result library (ErrorOr, OneOf, or custom implementation)
- SQLite for local development

## 🎨 Code Formatting Guidelines

### General Formatting
- **Indentation:** 4 spaces for all C# files (always 4 spaces, even for wrapping)
- **Braces:** Opening braces on the same line (K&R style)
- **Quotes:** Single quotes for character literals, double quotes for strings
- **Semicolons:** Always required for C# statements
- **Line Length:** 80 characters maximum (hard limit)
- **Exception:** Long URLs and string literals are exempt from 80-column limit and should not be wrapped

### Using Statements & Imports
- Group using statements by type with blank lines between groups:
  ```csharp
  // System namespaces
  using System;
  using System.Collections.Generic;
  
  // Third-party libraries
  using Microsoft.AspNetCore.Mvc;
  
  // Project namespaces
  using RequirementsVillage.Api.Models;
  ```

### Method Parameters & Arguments
- **Parameter Lists:** If more than 2 parameters, place each on its own line with aligned formatting:
  ```csharp
  // 2 or fewer parameters - single line
  public void ProcessRequest(string id, bool isActive)
  
  // 3+ parameters - multi-line with aligned formatting
  public async Task<ActionResult> CreateProject(
      string        name,
      string        description,
      ProjectStatus status,
      DateTime      createdAt
  ) {
  ```

### Object Initialization & Method Calls
- **Property Alignment:** For object initializers and method calls with multiple arguments:
  ```csharp
  var project = new Project {
      Id          = Guid.NewGuid(),
      Name        = "Sample Project",
      Description = "A sample project description",
      Status      = ProjectStatus.Active,
      CreatedAt   = DateTime.UtcNow
  };
  ```

### Attribute Alignment
- **Multiple Attributes:** Place each attribute on its own line:
  ```csharp
  [HttpPost]
  [Route("api/projects")]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectRequest request)
  ```

### Status
- **✅ Applied:** All formatting guidelines have been applied to the entire API codebase
- **📁 Scope:** All `.cs` files in the API project