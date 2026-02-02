using ProjectManager.Models;

namespace ProjectManager.Services.Interfaces;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(Guid id);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Guid id);
    Task UpdateProjectLinksAsync(Guid employeeId, List<Guid> projectIds);
}