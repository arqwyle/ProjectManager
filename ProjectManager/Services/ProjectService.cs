using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public async Task<List<Project>> GetAllAsync(
        string? name = null,
        string? customerName = null,
        string? executorName = null,
        DateTime? startTimeFrom = null,
        DateTime? startTimeTo = null,
        List<int>? priorities = null,
        Guid? directorId = null,
        string? sortBy = null,
        bool isSortAscending = true)
        => await projectRepository.GetAllAsync(
            name,
            customerName, 
            executorName, 
            startTimeFrom, 
            startTimeTo, 
            priorities, 
            directorId, 
            sortBy, 
            isSortAscending);

    public async Task<Project?> GetByIdAsync(Guid id)
        => await projectRepository.GetByIdAsync(id);

    public async Task AddAsync(Project project)
        => await projectRepository.AddAsync(project);

    public async Task UpdateAsync(Project project)
        => await projectRepository.UpdateAsync(project);

    public async Task DeleteAsync(Guid id)
        => await projectRepository.DeleteAsync(id);
    
    public async Task AddEmployeeToProjectAsync(Guid projectId, Guid employeeId)
        => await projectRepository.AddEmployeeToProjectAsync(projectId, employeeId);

    public async Task UpdateEmployeeLinksAsync(Guid projectId, List<Guid> employeeIds)
        => await projectRepository.UpdateEmployeeLinksAsync(projectId, employeeIds);
    
    public async Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId)
    => await projectRepository.RemoveEmployeeFromProjectAsync(projectId, employeeId);
    
    public async Task AddObjectiveToProjectAsync(Guid projectId, Guid objectiveId)
        => await projectRepository.AddObjectiveToProjectAsync(projectId, objectiveId);
    
    public async Task RemoveObjectiveFromProjectAsync(Guid projectId, Guid objectiveId)
        => await projectRepository.RemoveObjectiveFromProjectAsync(projectId, objectiveId);

    public async Task<List<Project>> GetManagerProjectsAsync(Guid employeeId)
        => await projectRepository.GetProjectsByDirectorIdAsync(employeeId);

    public async Task<List<Project>> GetEmployeeProjectsAsync(Guid employeeId)
        => await projectRepository.GetProjectsByEmployeeIdAsync(employeeId);
}