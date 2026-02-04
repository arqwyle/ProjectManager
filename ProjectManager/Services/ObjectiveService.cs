using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ObjectiveService(
    IObjectiveRepository objectiveRepository, 
    IProjectRepository projectRepository) : IObjectiveService
{
    public async Task<List<Objective>> GetAllAsync(
        string? name = null,
        List<Status>? statuses = null,
        List<int>? priorities = null,
        Guid? authorId = null,
        Guid? executorId = null,
        Guid? projectId = null,
        string? sortBy = null,
        bool isSortAscending = true)
        => await objectiveRepository.GetAllAsync(
            name,
            statuses, 
            priorities, 
            authorId, 
            executorId, 
            projectId, 
            sortBy, 
            isSortAscending);

    public async Task<Objective?> GetByIdAsync(Guid id)
        => await objectiveRepository.GetByIdAsync(id);

    public async Task AddAsync(Objective objective)
        => await objectiveRepository.AddAsync(objective);

    public async Task UpdateAsync(Objective objective)
        => await objectiveRepository.UpdateAsync(objective);

    public async Task DeleteAsync(Guid id)
        => await objectiveRepository.DeleteAsync(id);

    public async Task<List<Objective>> GetEmployeeObjectivesAsync(Guid employeeId)
        => await objectiveRepository.GetObjectivesByEmployeeIdAsync(employeeId);

    public async Task<List<Objective>> GetObjectivesForManagerProjectsAsync(Guid employeeId)
        => await objectiveRepository.GetObjectivesByDirectorIdAsync(employeeId);

    public async Task<bool> IsEmployeeInObjectiveProjectAsync(Objective objective, Guid employeeId)
        => (await projectRepository.GetProjectIdsByEmployeeIdAsync(employeeId)).Contains(objective.ProjectId!.Value);

    public async Task<bool> UpdateObjectiveStatusAsync(Guid objectiveId, Status status, Guid employeeId, bool isDirector)
    {
        var objective = await objectiveRepository.GetByIdAsync(objectiveId);
        if (objective == null)
            return false;

        if (isDirector || objective.ExecutorId == employeeId)
            return await ApplyStatusUpdate(objective, status);

        if (!objective.ProjectId.HasValue) 
            return false;
        
        var project = await projectRepository.GetByIdAsync(objective.ProjectId.Value);
        if (project?.DirectorId == employeeId)
            return await ApplyStatusUpdate(objective, status);

        return false;
    }

    private async Task<bool> ApplyStatusUpdate(Objective objective, Status status)
    {
        objective.Status = status;
        await objectiveRepository.UpdateAsync(objective);
        return true;
    }
}