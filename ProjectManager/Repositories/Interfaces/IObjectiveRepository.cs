using ProjectManager.Models;

namespace ProjectManager.Repositories.Interfaces;

public interface IObjectiveRepository
{
    Task<List<Objective>> GetAllAsync(
        List<Status>? statuses = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true);
    Task<Objective?> GetByIdAsync(Guid id);
    Task AddAsync(Objective objective);
    Task UpdateAsync(Objective objective);
    Task DeleteAsync(Guid id);
    Task<bool> IsEmployeeInObjectiveProjectAsync(Guid objectiveId, Guid employeeId);
}