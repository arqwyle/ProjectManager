using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ObjectiveService(IObjectiveRepository repository) : IObjectiveService
{
    public async Task<List<Objective>> GetAllAsync(
        List<Status>? statuses = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true)
        => await repository.GetAllAsync(statuses, priorities, sortBy, isSortAscending);

    public async Task<Objective?> GetByIdAsync(Guid id)
        => await repository.GetByIdAsync(id);

    public async Task AddAsync(Objective objective)
        => await repository.AddAsync(objective);

    public async Task UpdateAsync(Objective objective)
        => await repository.UpdateAsync(objective);

    public async Task DeleteAsync(Guid id)
        => await repository.DeleteAsync(id);
    
    public async Task<bool> IsEmployeeInObjectiveProjectAsync(Guid objectiveId, Guid employeeId)
        => await repository.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);
}