# Requirements Village - API

## Overview

A simple C# Minimal API backend for Requirements Village. Currently provides basic hello world endpoints with CORS enabled for frontend development.

## Tech Stack

- **Framework:** .NET 8 Minimal API
- **Language:** C# 
- **Database:** SQLite (planned)
- **ORM:** Entity Framework Core (planned)
- **Authentication:** ASP.NET Core Identity (planned)

## Development

### Prerequisites

- .NET 8 SDK

### Setup

```bash
cd api/RequirementsVillage.Api
dotnet restore
dotnet run
```

The API will be available at `https://localhost:7139` (or check console output for exact port)

### Available Endpoints

- `GET /` - Returns API name
- `GET /api/health` - Health check endpoint with timestamp

### Project Structure

```
api/
└── RequirementsVillage.Api/
    ├── Program.cs          # Main application entry point
    └── RequirementsVillage.Api.csproj
```

## Planned Features

- Entity Framework Core with SQLite
- ASP.NET Core Identity for authentication
- Project management endpoints
- Functional programming patterns with Result/Option types