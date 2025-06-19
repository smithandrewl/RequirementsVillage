using RequirementsVillage.Domain.Interfaces;
using RequirementsVillage.Domain.Services;
using RequirementsVillage.Persistence.Interfaces;
using RequirementsVillage.Persistence.Repositories;

namespace RequirementsVillage.Api.Extensions;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services
    ) {
        // Register repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        
        // Register domain services
        services.AddScoped<IProjectService, ProjectService>();
        
        return services;
    }
}