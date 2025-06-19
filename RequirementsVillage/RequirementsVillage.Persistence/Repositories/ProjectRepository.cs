using RequirementsVillage.Models;
using RequirementsVillage.Persistence.Interfaces;

namespace RequirementsVillage.Persistence.Repositories;

public class ProjectRepository : IProjectRepository {
    public Task<Seq<Project>> GetAllAsync() {
        var projects = Seq(
            new Project(
                Id:          Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                Name:        "Task Management System",
                Description: "A comprehensive task tracking application",
                Category:    "Productivity",
                Status:      "In Progress",
                CreatedAt:   DateTime.UtcNow.AddDays(-30),
                UpdatedAt:   DateTime.UtcNow.AddDays(-2)
            ),
            new Project(
                Id:          Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                Name:        "Weather Widget",
                Description: "A beautiful weather widget that nobody asked for",
                Category:    "Utilities",
                Status:      "Abandoned",
                CreatedAt:   DateTime.UtcNow.AddDays(-90),
                UpdatedAt:   DateTime.UtcNow.AddDays(-85)
            )
        );
        
        return Task.FromResult(projects);
    }
}