# CLAUDE.md

## 🏗️ Project Overview

**Project Name:** Requirements Village  
**Tagline:** *Where project ideas get laid to rest*  
**Purpose:** A Blazor Server application designed to help users catalog, organize, and reflect on their myriad project ideas—especially those that may never come to fruition. It serves as a structured repository for the indecisive, allowing for detailed specification without the commitment of execution.

---

## 🧰 Tech Stack

- **Framework:** .NET 8.0
- **Frontend:** Blazor Server
- **Backend:** ASP.NET Core
- **Data Access:** Entity Framework Core (Code-First)
- **Authentication:** ASP.NET Core Identity
- **Database:** SQLite
- **Containerization:** Docker (optional for deployment)
- **Operating System Compatibility:** Windows, Linux, macOS

---

## 🗂️ Solution & Project Structure

**Solution File:** `RequirementsVillage.sln`

**Projects:**

1. `RequirementsVillage.Web`  
   - **Type:** Blazor Server App  
   - **Purpose:** Hosts the UI and handles HTTP requests.  
   - **Dependencies:** `RequirementsVillage.Application`, `RequirementsVillage.Infrastructure`, `RequirementsVillage.Identity`

2. `RequirementsVillage.Application`  
   - **Type:** Class Library  
   - **Purpose:** Contains business logic, use cases, and service interfaces.

3. `RequirementsVillage.Infrastructure`  
   - **Type:** Class Library  
   - **Purpose:** Implements data access using EF Core, including DbContext and migrations.

4. `RequirementsVillage.Identity`  
   - **Type:** Class Library  
   - **Purpose:** Manages user authentication and authorization using ASP.NET Core Identity.

5. `RequirementsVillage.Shared`  
   - **Type:** Class Library  
   - **Purpose:** Contains shared models, DTOs, and utility classes.

---

## 🗃️ Database Configuration

- **Database Name:** `RequirementsVillage.db`
- **Provider:** SQLite
- **Initialization:** Ensure migrations are applied at startup. Use the following commands for migration management:

  ```bash
  dotnet ef migrations add InitialCreate --project RequirementsVillage.Infrastructure --startup-project RequirementsVillage.Web
  dotnet ef database update --project RequirementsVillage.Infrastructure --startup-project RequirementsVillage.Web
  ```

---

## 🧪 Testing

- **Testing Framework:** xUnit
- **Test Project:** `RequirementsVillage.Tests`
- **Test Coverage:** Focus on Application and Infrastructure layers.
- **Running Tests:** Use the following command to execute tests:

  ```bash
  dotnet test RequirementsVillage.Tests
  ```

---

## 🚀 Running the Application

To run the application locally:

```bash
dotnet run --project RequirementsVillage.Web
```

Access the application at `https://localhost:5001` or `http://localhost:5000`.

---

## 🐳 Docker Support

For containerized deployment:

1. **Build the Docker image:**

   ```bash
   docker build -t requirements-village -f RequirementsVillage.Web/Dockerfile .
   ```

2. **Run the Docker container:**

   ```bash
   docker run -d -p 5000:80 --name requirements-village requirements-village
   ```

Access the application at `http://localhost:5000`.

---

## 🧭 Project Features

- **User Management:**
  - Account registration and login
  - Password reset functionality
  - Role-based access control (optional)

- **Project Idea Management:**
  - Create, edit, and delete project ideas
  - Categorize ideas (e.g., "Someday", "In Progress", "Completed")
  - Assign tags and notes to each idea
  - Search and filter functionality

- **Analytics (Future Enhancement):**
  - Visualize the distribution of project statuses
  - Track the evolution of ideas over time

---

## 📝 Coding Standards & Guidelines

- **C# Features:**
  - Utilize `record` types for immutable data models
  - Prefer expression-bodied members for concise code
  - Use `async`/`await` for asynchronous operations

- **Project Structure:**
  - Maintain separation of concerns between layers
  - Keep controllers thin; delegate logic to services

- **Naming Conventions:**
  - Classes: PascalCase
  - Interfaces: Prefix with 'I' (e.g., `IProjectService`)
  - Methods and variables: camelCase

---

## 🔧 Common Commands

- **Restore NuGet packages:**

  ```bash
  dotnet restore
  ```

- **Build the solution:**

  ```bash
  dotnet build
  ```

- **Run the application:**

  ```bash
  dotnet run --project RequirementsVillage.Web
  ```

- **Apply EF Core migrations:**

  ```bash
  dotnet ef database update --project RequirementsVillage.Infrastructure --startup-project RequirementsVillage.Web
  ```

- **Run tests:**

  ```bash
  dotnet test RequirementsVillage.Tests
  ```

---

## 📂 Folder Structure

```
RequirementsVillage/
├── RequirementsVillage.sln
├── RequirementsVillage.Web/
│   ├── Pages/
│   ├── wwwroot/
│   └── Program.cs
├── RequirementsVillage.Application/
│   ├── Interfaces/
│   └── Services/
├── RequirementsVillage.Infrastructure/
│   ├── Data/
│   └── Migrations/
├── RequirementsVillage.Identity/
│   └── IdentitySetup/
├── RequirementsVillage.Shared/
│   └── Models/
└── RequirementsVillage.Tests/
    └── UnitTests/
```

---

## 🧠 Additional Notes

- **Deployment:** While Docker support is provided, the application is designed to run seamlessly on any environment supporting .NET 8.0.
- **Extensibility:** The modular architecture allows for easy addition of new features, such as integration with external APIs or advanced analytics.