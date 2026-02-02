using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class ObjectiveRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Guid _objectiveId1;
    private readonly Guid _objectiveId2;
    private readonly Guid _projectId;
    private readonly Guid _directorId;

    public ObjectiveRepositoryTests()
    {
        _objectiveId1 = Guid.NewGuid();
        _objectiveId2 = Guid.NewGuid();
        _projectId = Guid.NewGuid();
        _directorId = Guid.NewGuid();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _options = builder.Options;

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        using var context = new AppDbContext(_options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var project = new Project
        {
            Id = _projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = _directorId
        };

        var objective1 = new Objective
        {
            Id = _objectiveId1,
            Name = "1",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid(),
            ProjectId = _projectId
        };

        var objective2 = new Objective
        {
            Id = _objectiveId2,
            Name = "2",
            Priority = 2,
            Status = Status.InProgress,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid(),
            ProjectId = _projectId
        };

        context.Projects.Add(project);
        context.Objectives.AddRange(objective1, objective2);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllObjectives_WhenNoFilters()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync();

        Assert.Equal(2, objectives.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatuses()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(statuses: [Status.ToDo]);

        Assert.Single(objectives);
        Assert.Equal(_objectiveId1, objectives[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByPriorities()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(priorities: [2]);

        Assert.Single(objectives);
        Assert.Equal(_objectiveId2, objectives[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatusesAndPriorities()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(
            statuses: [Status.InProgress],
            priorities: [2]
        );

        Assert.Single(objectives);
        Assert.Equal(_objectiveId2, objectives[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldSortByNameAsc()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(sortBy: "name", isSortAscending: true);

        Assert.Equal("1", objectives[0].Name);
        Assert.Equal("2", objectives[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldSortByNameDesc()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(sortBy: "name", isSortAscending: false);

        Assert.Equal("2", objectives[0].Name);
        Assert.Equal("1", objectives[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldSortByPriorityAsc()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(sortBy: "priority", isSortAscending: true);

        Assert.Equal(1, objectives[0].Priority);
        Assert.Equal(2, objectives[1].Priority);
    }

    [Fact]
    public async Task GetAllAsync_ShouldSortByStatusAsc()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(sortBy: "status", isSortAscending: true);

        Assert.Equal(Status.ToDo, objectives[0].Status);
        Assert.Equal(Status.InProgress, objectives[1].Status);
    }

    [Fact]
    public async Task GetAllAsync_ShouldSortByIdWhenInvalidSortBy()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetAllAsync(sortBy: "invalid", isSortAscending: true);

        var ids = objectives.Select(o => o.Id).ToList();
        var sortedIds = ids.OrderBy(x => x).ToList();
        Assert.Equal(sortedIds, ids);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnObjective_WhenExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objective = await repository.GetByIdAsync(_objectiveId1);

        Assert.NotNull(objective);
        Assert.Equal(_objectiveId1, objective.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objective = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(objective);
    }

    [Fact]
    public async Task AddAsync_ShouldAddObjectiveToDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);
        var newId = Guid.NewGuid();
        var newObjective = new Objective
        {
            Id = newId,
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid()
        };

        await repository.AddAsync(newObjective);
        var saved = await context.Objectives.FindAsync(newId);

        Assert.NotNull(saved);
        Assert.Equal(newId, saved.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateObjectiveInDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);
        var objective = await context.Objectives.FindAsync(_objectiveId1);
        Assert.NotNull(objective);
        objective.Name = "Updated";

        await repository.UpdateAsync(objective);
        var updated = await context.Objectives.FindAsync(_objectiveId1);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveObjectiveFromDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        await repository.DeleteAsync(_objectiveId1);
        var deleted = await context.Objectives.FindAsync(_objectiveId1);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenObjectiveNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        await repository.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task GetObjectivesByDirectorIdAsync_ShouldReturnObjectivesForDirector()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetObjectivesByDirectorIdAsync(_directorId);

        Assert.Equal(2, objectives.Count);
        Assert.All(objectives, o => Assert.Equal(_projectId, o.ProjectId));
        Assert.All(objectives, o => Assert.NotNull(o.Project));
    }

    [Fact]
    public async Task GetObjectivesByDirectorIdAsync_ShouldReturnEmpty_WhenNoObjectives()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetObjectivesByDirectorIdAsync(Guid.NewGuid());

        Assert.Empty(objectives);
    }

    [Fact]
    public async Task GetObjectiveByIdAndAssigneeAsync_ShouldReturnObjective_WhenMatch()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);
        var executorId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();

        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = executorId
        };

        context.Objectives.Add(objective);
        await context.SaveChangesAsync();

        var result = await repository.GetObjectiveByIdAndAssigneeAsync(objectiveId, executorId);

        Assert.NotNull(result);
        Assert.Equal(objectiveId, result.Id);
    }

    [Fact]
    public async Task GetObjectiveByIdAndAssigneeAsync_ShouldReturnNull_WhenExecutorMismatch()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var result = await repository.GetObjectiveByIdAndAssigneeAsync(_objectiveId1, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetObjectiveByIdAndAssigneeAsync_ShouldReturnNull_WhenObjectiveNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var result = await repository.GetObjectiveByIdAndAssigneeAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetObjectivesByEmployeeIdAsync_ShouldReturnObjectivesForEmployee()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);
        var executorId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = executorId
        };

        context.Objectives.Add(objective);
        await context.SaveChangesAsync();

        var objectives = await repository.GetObjectivesByEmployeeIdAsync(executorId);

        Assert.Single(objectives);
        Assert.Equal(executorId, objectives[0].ExecutorId);
    }

    [Fact]
    public async Task GetObjectivesByEmployeeIdAsync_ShouldReturnEmpty_WhenNoObjectives()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ObjectiveRepository(context);

        var objectives = await repository.GetObjectivesByEmployeeIdAsync(Guid.NewGuid());

        Assert.Empty(objectives);
    }
}