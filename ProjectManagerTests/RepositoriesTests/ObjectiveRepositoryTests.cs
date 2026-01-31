using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class ObjectiveRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly ObjectiveRepository _repository;

    public ObjectiveRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new ObjectiveRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllObjectives()
    {
        var objective1 = new Objective { Id = Guid.NewGuid(), Name = "Test1", AuthorId = Guid.NewGuid(), Comment = "Test1", Priority = 1, Status = Status.ToDo, ProjectId = Guid.NewGuid() };
        var objective2 = new Objective { Id = Guid.NewGuid(), Name = "Test2", AuthorId = Guid.NewGuid(), Comment = "Test2", Priority = 2, Status = Status.InProgress, ProjectId = Guid.NewGuid() };
        await _context.Objectives.AddRangeAsync(objective1, objective2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, o => o.Id == objective1.Id);
        Assert.Contains(result, o => o.Id == objective2.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnObjective_WhenExists()
    {
        var id = Guid.NewGuid();
        var objective = new Objective { Id = id, Name = "Test", AuthorId = Guid.NewGuid(), Comment = "Test", Priority = 1, Status = Status.ToDo, ProjectId = Guid.NewGuid() };
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var id = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddObjective()
    {
        var objective = new Objective { Id = Guid.NewGuid(), Name = "Test", AuthorId = Guid.NewGuid(), Comment = "Test", Priority = 1, Status = Status.ToDo, ProjectId = Guid.NewGuid() };

        await _repository.AddAsync(objective);

        var exists = await _context.Objectives.AnyAsync(o => o.Id == objective.Id);
        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateObjective()
    {
        var objective = new Objective { Id = Guid.NewGuid(), Name = "Old", AuthorId = Guid.NewGuid(), Comment = "Old", Priority = 0, Status = Status.ToDo, ProjectId = Guid.NewGuid() };
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        objective.Name = "New";
        objective.Comment = "New";
        objective.Priority = 2;

        await _repository.UpdateAsync(objective);
        await _context.SaveChangesAsync();

        var updated = await _context.Objectives.FindAsync(objective.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.Name);
        Assert.Equal("New", updated.Comment);
        Assert.Equal(2, updated.Priority);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveObjective_WhenExists()
    {
        var objective = new Objective { Id = Guid.NewGuid(), Name = "Test", AuthorId = Guid.NewGuid(), Comment = "Test", Priority = 1, Status = Status.ToDo, ProjectId = Guid.NewGuid() };
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(objective.Id);

        var exists = await _context.Objectives.AnyAsync(o => o.Id == objective.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenNotExists()
    {
        var id = Guid.NewGuid();

        await _repository.DeleteAsync(id);

        // Should not throw exception
        Assert.True(true);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnTrue_WhenEmployeeInProject()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var objective = new Objective { Id = objectiveId, Name = "Test", AuthorId = Guid.NewGuid(), Comment = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId };
        var employeeProject = new EmployeeProject { ProjectId = projectId, EmployeeId = employeeId };

        await _context.Objectives.AddAsync(objective);
        await _context.EmployeeProjects.AddAsync(employeeProject);
        await _context.SaveChangesAsync();

        var result = await _repository.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnFalse_WhenEmployeeNotInProject()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var objective = new Objective { Id = objectiveId, Name = "Test", AuthorId = Guid.NewGuid(), Comment = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId };

        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        var result = await _repository.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnFalse_WhenObjectiveNotExists()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var result = await _repository.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.False(result);
    }
}