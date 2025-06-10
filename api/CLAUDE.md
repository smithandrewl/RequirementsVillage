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