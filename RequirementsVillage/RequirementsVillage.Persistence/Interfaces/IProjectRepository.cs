using RequirementsVillage.Models;

namespace RequirementsVillage.Persistence.Interfaces;

public interface IProjectRepository {
    Task<Seq<Project>> GetAllAsync();
}