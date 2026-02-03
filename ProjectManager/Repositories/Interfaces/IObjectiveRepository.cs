using ProjectManager.Models;

namespace ProjectManager.Repositories.Interfaces;

public interface IObjectiveRepository
{
    Task<List<Objective>> GetAllAsync(
        string? name = null,
        List<Status>? statuses = null,
        List<int>? priorities = null,
        Guid? authorId = null,
        Guid? executorId = null,
        Guid? projectId = null,
        string? sortBy = null,
        bool isSortAscending = true);
    Task<Objective?> GetByIdAsync(Guid id);
    Task AddAsync(Objective objective);
    Task UpdateAsync(Objective objective);
    Task DeleteAsync(Guid id);
    Task<List<Objective>> GetObjectivesByDirectorIdAsync(Guid directorId);
    Task<Objective?> GetObjectiveByIdAndAssigneeAsync(Guid objectiveId, Guid? assigneeId);
    Task<List<Objective>> GetObjectivesByEmployeeIdAsync(Guid employeeId);
}