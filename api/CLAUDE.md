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

## 🚀 Planned Architecture

- **Functional C#** with Result/Option discriminated unions
- **Exhaustive pattern matching** for error handling
- **Immutable data structures** where possible
- **Clean separation** of concerns

## 📝 Development Notes

- CORS configured for frontend development (localhost:5173)
- Minimal dependencies for now - will expand incrementally
- Database and authentication features planned for later commits

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