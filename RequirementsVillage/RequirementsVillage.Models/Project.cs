namespace RequirementsVillage.Models;

public record Project(
    Guid     Id,
    string   Name,
    string   Description,
    string   Category,
    string   Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);