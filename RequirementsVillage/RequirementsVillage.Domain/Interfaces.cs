using RequirementsVillage.Models;

namespace RequirementsVillage.Domain;

public interface IProjectService {
    Task<Either<Error, Seq<Project>>> GetAllProjectsAsync();
}
