using RequirementsVillage.Models;
using RequirementsVillage.Persistence.Interfaces;

namespace RequirementsVillage.Persistence.Repositories;

public class ProjectRepository : IProjectRepository {
    public Task<Seq<Project>> GetAllAsync() {
        var projects = Seq(
            new Models.Project(
                Id:          Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                Name:        "Task Manager",
                Description: "A web-based task tracking application",
                Category:    "Productivity",
                Status:      "Someday",
                CreatedAt:   DateTime.UtcNow.AddDays(-30),
                UpdatedAt:   DateTime.UtcNow.AddDays(-2)
            ),
            new Project(
                Id:          Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                Name:        "Expense Tracker",
                Description: "An application for monitoring personal expenses",
                Category:    "Finance",
                Status:      "In Progress",
                CreatedAt:   DateTime.UtcNow.AddDays(-15),
                UpdatedAt:   DateTime.UtcNow.AddDays(-1)
            ),
            new Project(
                Id:          Guid.Parse("7ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                Name:        "Weather App",
                Description: "A weather forecasting application",
                Category:    "Utilities",
                Status:      "Someday",
                CreatedAt:   DateTime.UtcNow.AddDays(-60),
                UpdatedAt:   DateTime.UtcNow.AddDays(-45)
            ),
            new Project(
                Id:          Guid.Parse("8ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                Name:        "Blog Platform",
                Description: "A simple platform for creating and managing blogs",
                Category:    "Content",
                Status:      "Abandoned",
                CreatedAt:   DateTime.UtcNow.AddDays(-120),
                UpdatedAt:   DateTime.UtcNow.AddDays(-100)
            )
        );
        
        return Task.FromResult(projects);
    }
}
