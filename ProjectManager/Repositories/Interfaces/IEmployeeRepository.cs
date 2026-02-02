using ProjectManager.Models;

namespace ProjectManager.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<List<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Guid id);
    Task<Guid?> GetEmployeeIdByUserIdAsync(string userId);
    Task<List<Project>> GetProjectsByEmployeeIdAsync(Guid? employeeId);
    Task<List<Objective>> GetObjectivesByEmployeeIdAsync(Guid? employeeId);
    Task UpdateProjectLinksAsync(Guid employeeId, List<Guid> projectIds);
}