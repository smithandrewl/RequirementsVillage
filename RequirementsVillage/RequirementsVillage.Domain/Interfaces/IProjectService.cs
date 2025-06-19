using RequirementsVillage.Models;

namespace RequirementsVillage.Domain.Interfaces;

public interface IProjectService {
    Task<Either<Error, Seq<Project>>> GetAllProjectsAsync();
}