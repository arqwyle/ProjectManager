using ProjectManager.Models;

namespace ProjectManager.Services.Interfaces;

public interface IProjectService
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
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
    Task AddEmployeeToProjectAsync(Guid projectId, Guid employeeId);
    Task UpdateEmployeeLinksAsync(Guid projectId, List<Guid> employeeIds);
    Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId);
    Task AddObjectiveToProjectAsync(Guid projectId, Guid objectiveId);
    Task RemoveObjectiveFromProjectAsync(Guid projectId, Guid objectiveId);
    Task<Guid?> GetEmployeeIdByUserId(string userId);
    Task<List<Project>> GetManagerProjectsAsync(string userName);
    Task<List<Project>> GetEmployeeProjectsAsync(string userId);
}