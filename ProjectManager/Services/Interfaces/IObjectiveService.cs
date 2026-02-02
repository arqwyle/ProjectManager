using ProjectManager.Models;

namespace ProjectManager.Services.Interfaces;

public interface IObjectiveService
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
    Task<Guid?> GetEmployeeIdByUserId(string userId);
    Task<List<Objective>> GetObjectivesForManagerProjectsAsync(string userName);
    Task<bool> UpdateObjectiveStatusAsync(Guid objectiveId, Status status, string userName, List<string> roles);
    Task<List<Objective>> GetEmployeeObjectivesAsync(string userId);
}