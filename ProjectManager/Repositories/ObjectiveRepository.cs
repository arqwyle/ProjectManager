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
            "priority" => isAsc ? query.OrderBy(o => o.Priority) : query.OrderByDescending(o => o.Priority),
            "status" => isAsc ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            _ => isAsc ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id)
        };
    }
    
    public async Task<List<Objective>> GetAllAsync(
        List<Status>? statuses = null,
        List<int>? priorities = null,
        string? sortBy = null,
        bool isSortAscending = true)
    {
        IQueryable<Objective> query = context.Objectives;

        if (statuses?.Count > 0)
            query = query.Where(o => statuses.Contains(o.Status));
        if (priorities?.Count > 0)
            query = query.Where(o => priorities.Contains(o.Priority));

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