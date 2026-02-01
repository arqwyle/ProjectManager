using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ObjectiveService(IObjectiveRepository objectiveRepository, IEmployeeRepository employeeRepository) : IObjectiveService
{
    public async Task<List<Objective>> GetAllAsync(
        List<Status>? statuses = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true)
        => await objectiveRepository.GetAllAsync(statuses, priorities, sortBy, isSortAscending);

    public async Task<Objective?> GetByIdAsync(Guid id)
        => await objectiveRepository.GetByIdAsync(id);

    public async Task AddAsync(Objective objective)
        => await objectiveRepository.AddAsync(objective);

    public async Task UpdateAsync(Objective objective)
        => await objectiveRepository.UpdateAsync(objective);

    public async Task DeleteAsync(Guid id)
        => await objectiveRepository.DeleteAsync(id);
    
    public async Task<bool> IsEmployeeInObjectiveProjectAsync(Guid objectiveId, Guid employeeId)
        => await objectiveRepository.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);
    
    public async Task<List<Objective>> GetObjectivesForManagerProjectsAsync(string userId)
    {
        var employeeId = await employeeRepository.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return [];

        return await objectiveRepository.GetObjectivesByDirectorIdAsync(employeeId.Value);
    }
    
    public async Task<bool> UpdateObjectiveStatusAsync(Guid objectiveId, Status status, string userId, List<string> roles)
    {
        if (roles.Contains("руководитель"))
            return await UpdateObjectiveStatusInternal(objectiveId, status);

        if (!roles.Contains("менеджер проектов") && !roles.Contains("сотрудник"))
            return false;

        var employeeId = await employeeRepository.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null) return false;

        if (roles.Contains("сотрудник"))
            return await UpdateObjectiveIfAssignedToEmployee(objectiveId, status, employeeId.Value);

        if (roles.Contains("менеджер проектов"))
            return await UpdateObjectiveIfManagerInProject(objectiveId, status, employeeId.Value);

        return false;
    }

    private async Task<bool> UpdateObjectiveStatusInternal(Guid objectiveId, Status status)
    {
        var objective = await objectiveRepository.GetByIdAsync(objectiveId);
        if (objective == null) 
            return false;
        
        objective.Status = status;
        await objectiveRepository.UpdateObjectiveAsync(objective);
        return true;
    }

    private async Task<bool> UpdateObjectiveIfAssignedToEmployee(Guid objectiveId, Status status, Guid employeeId)
    {
        var objective = await objectiveRepository.GetObjectiveByIdAndAssigneeAsync(objectiveId, employeeId);
        if (objective == null) 
            return false;
        
        objective.Status = status;
        await objectiveRepository.UpdateObjectiveAsync(objective);
        return true;
    }

    private async Task<bool> UpdateObjectiveIfManagerInProject(Guid objectiveId, Status status, Guid employeeId)
    {
        var objective = await objectiveRepository.GetByIdAsync(objectiveId);
        if (objective == null) 
            return false;
        
        var isInProject = await employeeRepository.IsEmployeeInProjectAsync(employeeId, objective.ProjectId);
        if (!isInProject) 
            return false;
        
        objective.Status = status;
        await objectiveRepository.UpdateObjectiveAsync(objective);
        return true;
    }
}