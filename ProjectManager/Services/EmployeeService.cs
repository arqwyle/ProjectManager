using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services;

public class EmployeeService(IEmployeeRepository repository) : IEmployeeService
{
    public async Task<List<Employee>> GetAllAsync()
        => await repository.GetAllAsync();

    public async Task<Employee?> GetByIdAsync(Guid id)
        => await repository.GetByIdAsync(id);

    public async Task AddAsync(Employee employee)
        => await repository.AddAsync(employee);

    public async Task UpdateAsync(Employee employee)
        => await repository.UpdateAsync(employee);

    public async Task DeleteAsync(Guid id)
        => await repository.DeleteAsync(id);
    
    public async Task<Guid?> GetEmployeeIdByUserIdAsync(string userId)
        => await repository.GetEmployeeIdByUserIdAsync(userId);
}