using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;

namespace ProjectManager.Repositories;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await context.Employees.FindAsync(id);
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await context.Employees.ToListAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee != null)
        {
            context.Employees.Remove(employee);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<Guid?> GetEmployeeIdByUserIdAsync(string userId)
    {
        return await context.Employees
            .Where(e => e.UserId == userId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> IsEmployeeInProjectAsync(Guid employeeId, Guid projectId)
    {
        return await context.EmployeeProjects
            .AnyAsync(ep => ep.EmployeeId == employeeId && ep.ProjectId == projectId);
    }
    
    public async Task<List<Project>> GetProjectsByEmployeeIdAsync(Guid? employeeId)
    {
        var employeeProjects = await context.EmployeeProjects
            .Where(ep => ep.EmployeeId == employeeId)
            .Include(ep => ep.Project)
            .ToListAsync();

        return employeeProjects.Select(ep => ep.Project).ToList();
    }

    public async Task<List<Objective>> GetObjectivesByEmployeeIdAsync(Guid? employeeId)
    {
        return await context.Objectives
            .Where(o => o.ExecutorId == employeeId)
            .ToListAsync();
    }
}