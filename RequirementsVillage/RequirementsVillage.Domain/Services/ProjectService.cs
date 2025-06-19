using RequirementsVillage.Domain.Interfaces;
using RequirementsVillage.Models;
using RequirementsVillage.Persistence.Interfaces;

namespace RequirementsVillage.Domain.Services;

public class ProjectService : IProjectService {
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository) {
        _repository = repository;
    }

    public async Task<Either<Error, Seq<Project>>> GetAllProjectsAsync() {
        try {
            var projects = await _repository.GetAllAsync();
            return Right<Error, Seq<Project>>(projects);
        }
        catch (Exception ex) {
            return Left<Error, Seq<Project>>(
                Error.New($"Failed to retrieve projects: {ex.Message}")
            );
        }
    }
}