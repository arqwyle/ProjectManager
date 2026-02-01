using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;

namespace ProjectManager.Repositories;

public class ProjectRepository(AppDbContext context) : IProjectRepository
{
    private static IQueryable<Project> ApplySorting(IQueryable<Project> query, string? sortBy, bool isAsc)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => isAsc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "customername" => isAsc ? query.OrderBy(p => p.CustomerName) : query.OrderByDescending(p => p.CustomerName),
            "executorname" => isAsc ? query.OrderBy(p => p.ExecutorName) : query.OrderByDescending(p => p.ExecutorName),
            "starttime" => isAsc ? query.OrderBy(p => p.StartTime) : query.OrderByDescending(p => p.StartTime),
            "endtime" => isAsc ? query.OrderBy(p => p.EndTime) : query.OrderByDescending(p => p.EndTime),
            "priority" => isAsc ? query.OrderBy(p => p.Priority) : query.OrderByDescending(p => p.Priority),
            _ => isAsc ? query.OrderBy(p => p.StartTime) : query.OrderByDescending(p => p.StartTime)
        };
    }
    
    public async Task<List<Project>> GetAllAsync(
        string? customerName = null,
        string? executorName = null,
        DateTime? startTimeFrom = null,
        DateTime? startTimeTo = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true)
    {
        IQueryable<Project> query = context.Projects;
        
        if(!string.IsNullOrEmpty(customerName))
            query = query.Where(p => p.CustomerName == customerName);
        if(!string.IsNullOrEmpty(executorName))
            query = query.Where(p => p.ExecutorName == executorName);
        if (startTimeFrom.HasValue)
            query = query.Where(p => p.StartTime >= startTimeFrom.Value);
        if (startTimeTo.HasValue)
            query = query.Where(p => p.StartTime <= startTimeTo.Value);
        if (priorities?.Count > 0)
            query = query.Where(p => priorities.Contains(p.Priority));

        query = ApplySorting(query, sortBy, isSortAscending);
        return await query.ToListAsync();
    }
    
    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await context.Projects
            .Include(p => p.EmployeeProjects)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> AddAsync(Project project)
    {
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await context.Projects
            .Include(p => p.EmployeeProjects)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (project != null)
        {
            context.EmployeeProjects.RemoveRange(project.EmployeeProjects);
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task AddEmployeeToProjectAsync(Guid projectId, Guid employeeId)
    {
        var exists = await context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
            
        if (!exists)
        {
            context.EmployeeProjects.Add(new EmployeeProject
            {
                ProjectId = projectId,
                EmployeeId = employeeId
            });
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateEmployeeLinksAsync(Guid projectId, List<Guid> employeeIds)
    {
        var existingLinks = await context.EmployeeProjects
            .Where(ep => ep.ProjectId == projectId)
            .ToListAsync();

        foreach (var link in existingLinks.Where(l => !employeeIds.Contains(l.EmployeeId)))
            context.EmployeeProjects.Remove(link);

        foreach (var empId in employeeIds.Where(empId => existingLinks.All(l => l.EmployeeId != empId)))
        {
            context.EmployeeProjects.Add(new EmployeeProject 
            { 
                ProjectId = projectId, 
                EmployeeId = empId 
            });
        }

        await context.SaveChangesAsync();
    }
    
    public async Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId)
    {
        var link = await context.EmployeeProjects
            .FirstOrDefaultAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
            
        if (link != null)
        {
            context.EmployeeProjects.Remove(link);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task AddObjectiveToProjectAsync(Guid projectId, Guid objectiveId)
    {
        var objective = await context.Objectives.FindAsync(objectiveId);
        if (objective != null)
        {
            objective.ProjectId = projectId;
            context.Objectives.Update(objective);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveObjectiveFromProjectAsync(Guid projectId, Guid objectiveId)
    {
        var objective = await context.Objectives
            .FirstOrDefaultAsync(o => o.Id == objectiveId && o.ProjectId == projectId);
        if (objective != null)
        {
            context.Objectives.Remove(objective);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<Project>> GetProjectsByDirectorIdAsync(Guid directorId)
    {
        return await context.Projects
            .Where(ep => ep.DirectorId == directorId)
            .ToListAsync();
    }
}