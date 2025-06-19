using RequirementsVillage.Domain.Interfaces;

namespace RequirementsVillage.Api.Endpoints;

public static class ProjectEndpoints {
    public static void MapProjectEndpoints(this WebApplication app) {
        var projects = app.MapGroup("/api/projects");

        projects.MapGet("/", GetAllProjects)
            .WithName("GetAllProjects")
            .Produces(200);
    }

    private static async Task<IResult> GetAllProjects(
        IProjectService service
    ) {
        var result = await service.GetAllProjectsAsync();
        
        return result.Match(
            Left:  error => Results.Problem(error.Message),
            Right: projects => Results.Ok(projects)
        );
    }
}