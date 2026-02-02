using ProjectManager.Models;

namespace ProjectManager.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(
        string? customerName = null,
        string? executorName = null,
        DateTime? startTimeFrom = null,
        DateTime? startTimeTo = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true);
    Task<Project?> GetByIdAsync(Guid id);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
    Task AddEmployeeToProjectAsync(Guid projectId, Guid employeeId);
    Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId);
    Task UpdateEmployeeLinksAsync(Guid projectId, List<Guid> employeeIds);
    Task AddObjectiveToProjectAsync(Guid projectId, Guid objectiveId);
    Task RemoveObjectiveFromProjectAsync(Guid projectId, Guid objectiveId);
    Task<List<Project>> GetProjectsByDirectorIdAsync(Guid directorId);
    Task<List<Project>> GetProjectsByEmployeeIdAsync(Guid employeeId);
    Task<List<Guid>> GetProjectIdsByEmployeeIdAsync(Guid employeeId);
}