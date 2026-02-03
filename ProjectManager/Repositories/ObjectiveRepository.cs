using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;

namespace ProjectManager.Repositories;

public class ObjectiveRepository(AppDbContext context) : IObjectiveRepository
{
    private static IQueryable<Objective> ApplySorting(IQueryable<Objective> query, string? sortBy, bool isAsc)
    {
        return sortBy?.ToLower() switch
        {
            "name" => isAsc ? query.OrderBy(o => o.Name) : query.OrderByDescending(o => o.Name),
            "status" => isAsc ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            "priority" => isAsc ? query.OrderBy(o => o.Priority) : query.OrderByDescending(o => o.Priority),
            "author" => isAsc ? query.OrderBy(o => o.Author != null ? o.Author.LastName : string.Empty)
                : query.OrderByDescending(o => o.Author != null ? o.Author.LastName : string.Empty),
            "executor" => isAsc ? query.OrderBy(o => o.Executor != null ? o.Executor.LastName : string.Empty)
                : query.OrderByDescending(o => o.Executor != null ? o.Executor.LastName : string.Empty),
            "project" => isAsc ? query.OrderBy(o => o.Project != null ? o.Project.Name : string.Empty)
                : query.OrderByDescending(o => o.Project != null ? o.Project.Name : string.Empty),
            _ => isAsc ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id)
        };
    }
    
    public async Task<List<Objective>> GetAllAsync(
        string? name = null,
        List<Status>? statuses = null,
        List<int>? priorities = null,
        Guid? authorId = null,
        Guid? executorId = null,
        Guid? projectId = null,
        string? sortBy = null,
        bool isSortAscending = true)
    {
        IQueryable<Objective> query = context.Objectives
            .Include(o => o.Author)
            .Include(o => o.Executor)
            .Include(o => o.Project);
        
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(o => o.Name.Contains(name));
        if (statuses?.Count > 0)
            query = query.Where(o => statuses.Contains(o.Status));
        if (priorities?.Count > 0)
            query = query.Where(o => priorities.Contains(o.Priority));
        if (authorId.HasValue)
            query = query.Where(o => o.AuthorId == authorId.Value);
        if (executorId.HasValue)
            query = query.Where(o => o.ExecutorId == executorId.Value);
        if (projectId.HasValue)
            query = query.Where(o => o.ProjectId == projectId.Value);

        query = ApplySorting(query, sortBy, isSortAscending);
        return await query.ToListAsync();
    }
    
    public async Task<Objective?> GetByIdAsync(Guid id)
    {
        return await context.Objectives.FindAsync(id);
    }

    public async Task AddAsync(Objective objective)
    {
        await context.Objectives.AddAsync(objective);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Objective objective)
    {
        context.Objectives.Update(objective);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var objective = await context.Objectives.FindAsync(id);
        if (objective != null)
        {
            context.Objectives.Remove(objective);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<Objective>> GetObjectivesByDirectorIdAsync(Guid directorId)
    {
        return await context.Objectives
            .Include(o => o.Project)
            .Where(o => o.Project!.DirectorId == directorId)
            .ToListAsync();
    }
    
    public async Task<Objective?> GetObjectiveByIdAndAssigneeAsync(Guid objectiveId, Guid? assigneeId)
    {
        return await context.Objectives
            .FirstOrDefaultAsync(o => o.Id == objectiveId && o.ExecutorId == assigneeId);
    }

    public async Task<List<Objective>> GetObjectivesByEmployeeIdAsync(Guid employeeId)
    {
        return await context.Objectives
            .Where(o => o.ExecutorId == employeeId)
            .ToListAsync();
    }
}