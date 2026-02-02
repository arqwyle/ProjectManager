using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ObjectiveService(
    IObjectiveRepository objectiveRepository, 
    IEmployeeRepository employeeRepository,
    IProjectRepository projectRepository) : IObjectiveService
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

    public async Task<Guid?> GetEmployeeIdByUserId(string userId)
        => await employeeRepository.GetEmployeeIdByUserIdAsync(userId);

    public async Task<List<Objective>> GetObjectivesForManagerProjectsAsync(string userId)
    {
        var employeeId = await employeeRepository.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return [];

        return await objectiveRepository.GetObjectivesByDirectorIdAsync(employeeId.Value);
    }

    public async Task<bool> UpdateObjectiveStatusAsync(Guid objectiveId, Status status, string userId, List<string> roles)
    {
        if (roles.Contains("director"))
            return await UpdateObjectiveIfAllowed(objectiveId, status, _ => Task.FromResult(true));

        if (!roles.Contains("project manager") && !roles.Contains("employee"))
            return false;

        var employeeId = await GetEmployeeIdByUserId(userId);
        if (employeeId == null)
            return false;

        if (roles.Contains("employee"))
            return await UpdateObjectiveIfAllowed(
                objectiveId,
                status,
                async id =>
                {
                    var objective = await objectiveRepository.GetObjectiveByIdAndAssigneeAsync(id, employeeId.Value);
                    return objective != null;
                }
            );

        if (roles.Contains("project manager"))
            return await UpdateObjectiveIfAllowed(
                objectiveId,
                status,
                async id =>
                {
                    var objective = await objectiveRepository.GetByIdAsync(id);
                    if (objective?.ProjectId == null) 
                        return false;

                    var project = await projectRepository.GetByIdAsync((Guid)objective.ProjectId);
                    return project?.DirectorId == employeeId.Value;
                }
            );

        return false;
    }

    private async Task<bool> UpdateObjectiveIfAllowed(Guid objectiveId, Status status, Func<Guid, Task<bool>> canUpdate)
    {
        if (!await canUpdate(objectiveId))
            return false;

        var objective = await objectiveRepository.GetByIdAsync(objectiveId);
        if (objective == null)
            return false;

        objective.Status = status;
        await objectiveRepository.UpdateObjectiveAsync(objective);
        return true;
    }

    public async Task<List<Objective>> GetEmployeeObjectivesAsync(string userId)
    {
        var employeeId = await GetEmployeeIdByUserId(userId);
        if (employeeId == null) 
            return [];

        return await employeeRepository.GetObjectivesByEmployeeIdAsync(employeeId);
    }
}