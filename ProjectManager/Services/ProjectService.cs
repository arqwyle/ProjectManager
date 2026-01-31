using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ProjectService(IProjectRepository repository) : IProjectService
{
    public async Task<List<Project>> GetAllAsync(
        string? customerName = null,
        string? executorName = null,
        DateTime? startTimeFrom = null,
        DateTime? startTimeTo = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true)
        => await repository.GetAllAsync(customerName, executorName, startTimeFrom, startTimeTo, priorities, sortBy, isSortAscending);

    public async Task<Project?> GetByIdAsync(Guid id)
        => await repository.GetByIdAsync(id);

    public async Task<Project> AddAsync(Project project)
        => await repository.AddAsync(project);

    public async Task UpdateAsync(Project project)
        => await repository.UpdateAsync(project);

    public async Task DeleteAsync(Guid id)
        => await repository.DeleteAsync(id);
    
    public async Task AddEmployeeToProjectAsync(Guid projectId, Guid employeeId)
        => await repository.AddEmployeeToProjectAsync(projectId, employeeId);

    public async Task UpdateEmployeeLinksAsync(Guid projectId, List<Guid> employeeIds)
        => await repository.UpdateEmployeeLinksAsync(projectId, employeeIds);
    
    public async Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId)
    => await repository.RemoveEmployeeFromProjectAsync(projectId, employeeId);
    
    public async Task AddObjectiveToProjectAsync(Guid projectId, Guid objectiveId)
        => await repository.AddObjectiveToProjectAsync(projectId, objectiveId);
    
    public async Task RemoveObjectiveFromProjectAsync(Guid projectId, Guid objectiveId)
        => await repository.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
}