using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class ProjectRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Guid _projectId1;
    private readonly Guid _projectId2;
    private readonly Guid _employeeId1;
    private readonly Guid _employeeId2;
    private readonly Guid _directorId;
    private readonly DateTime _startTime;

    public ProjectRepositoryTests()
    {
        _projectId1 = Guid.NewGuid();
        _projectId2 = Guid.NewGuid();
        _employeeId1 = Guid.NewGuid();
        _employeeId2 = Guid.NewGuid();
        _directorId = Guid.NewGuid();
        _startTime = DateTime.Now.Date;

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

        var project1 = new Project
        {
            Id = _projectId1,
            Name = "1",
            CustomerName = "1",
            ExecutorName = "1",
            StartTime = _startTime,
            EndTime = _startTime.AddDays(10),
            Priority = 1,
            DirectorId = _directorId
        };

        var project2 = new Project
        {
            Id = _projectId2,
            Name = "Test2",
            CustomerName = "2",
            ExecutorName = "2",
            StartTime = _startTime.AddDays(5),
            EndTime = _startTime.AddDays(15),
            Priority = 2,
            DirectorId = Guid.NewGuid()
        };

        context.Projects.AddRange(project1, project2);

        context.EmployeeProjects.AddRange(
            new EmployeeProject { ProjectId = _projectId1, EmployeeId = _employeeId1 },
            new EmployeeProject { ProjectId = _projectId1, EmployeeId = _employeeId2 },
            new EmployeeProject { ProjectId = _projectId2, EmployeeId = _employeeId1 }
        );

        context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProject_WithEmployeeProjects()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var project = await repository.GetByIdAsync(_projectId1);

        Assert.NotNull(project);
        Assert.Equal(_projectId1, project.Id);
        Assert.Equal(2, project.EmployeeProjects.Count);
        Assert.Contains(_employeeId1, project.EmployeeProjects.Select(ep => ep.EmployeeId));
        Assert.Contains(_employeeId2, project.EmployeeProjects.Select(ep => ep.EmployeeId));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var project = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(project);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProjectToDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var newId = Guid.NewGuid();
        var newProject = new Project
        {
            Id = newId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        await repository.AddAsync(newProject);
        var saved = await context.Projects.FindAsync(newId);

        Assert.NotNull(saved);
        Assert.Equal(newId, saved.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProjectInDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var project = await context.Projects.FindAsync(_projectId1);
        Assert.NotNull(project);
        project.Name = "Updated";

        await repository.UpdateAsync(project);
        var updated = await context.Projects.FindAsync(_projectId1);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProjectAndEmployeeLinks()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.DeleteAsync(_projectId1);

        var deletedProject = await context.Projects.FindAsync(_projectId1);
        var remainingLinks = await context.EmployeeProjects
            .Where(ep => ep.ProjectId == _projectId1)
            .ToListAsync();

        Assert.Null(deletedProject);
        Assert.Empty(remainingLinks);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenProjectNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldAddLink_WhenNotExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var newEmployeeId = Guid.NewGuid();

        await repository.AddEmployeeToProjectAsync(_projectId2, newEmployeeId);

        var linkExists = await context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == _projectId2 && ep.EmployeeId == newEmployeeId);

        Assert.True(linkExists);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldDoNothing_WhenAlreadyExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.AddEmployeeToProjectAsync(_projectId1, _employeeId1);

        var count = await context.EmployeeProjects
            .CountAsync(ep => ep.ProjectId == _projectId1 && ep.EmployeeId == _employeeId1);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldRemoveLink_WhenExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.RemoveEmployeeFromProjectAsync(_projectId1, _employeeId1);

        var linkExists = await context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == _projectId1 && ep.EmployeeId == _employeeId1);

        Assert.False(linkExists);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldDoNothing_WhenNotExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.RemoveEmployeeFromProjectAsync(_projectId1, Guid.NewGuid());
    }

    [Fact]
    public async Task UpdateEmployeeLinksAsync_ShouldReplaceLinks()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var newEmployeeId = Guid.NewGuid();
        var newLinks = new List<Guid> { newEmployeeId };

        await repository.UpdateEmployeeLinksAsync(_projectId1, newLinks);

        var currentLinks = await context.EmployeeProjects
            .Where(ep => ep.ProjectId == _projectId1)
            .Select(ep => ep.EmployeeId)
            .ToListAsync();

        Assert.Single(currentLinks);
        Assert.Equal(newEmployeeId, currentLinks[0]);
    }

    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldSetProjectId()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var objectiveId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid()
        };

        context.Objectives.Add(objective);
        await context.SaveChangesAsync();

        await repository.AddObjectiveToProjectAsync(_projectId1, objectiveId);

        var updatedObjective = await context.Objectives.FindAsync(objectiveId);
        Assert.NotNull(updatedObjective);
        Assert.Equal(_projectId1, updatedObjective.ProjectId);
    }

    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldDoNothing_WhenObjectiveNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        await repository.AddObjectiveToProjectAsync(_projectId1, Guid.NewGuid());
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldRemoveOnlyIfMatchesProject()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var objectiveId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid(),
            ProjectId = _projectId1
        };

        context.Objectives.Add(objective);
        await context.SaveChangesAsync();

        await repository.RemoveObjectiveFromProjectAsync(_projectId1, objectiveId);

        var deleted = await context.Objectives.FindAsync(objectiveId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldNotRemove_IfProjectMismatch()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);
        var objectiveId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            Priority = 1,
            Status = Status.ToDo,
            AuthorId = Guid.NewGuid(),
            ExecutorId = Guid.NewGuid(),
            ProjectId = _projectId2
        };

        context.Objectives.Add(objective);
        await context.SaveChangesAsync();

        await repository.RemoveObjectiveFromProjectAsync(_projectId1, objectiveId);

        var stillExists = await context.Objectives.FindAsync(objectiveId);
        Assert.NotNull(stillExists);
    }

    [Fact]
    public async Task GetProjectsByDirectorIdAsync_ShouldReturnProjectsForDirector()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projects = await repository.GetProjectsByDirectorIdAsync(_directorId);

        Assert.Single(projects);
        Assert.Equal(_projectId1, projects[0].Id);
        Assert.NotNull(projects[0].EmployeeProjects);
    }

    [Fact]
    public async Task GetProjectsByDirectorIdAsync_ShouldReturnEmpty_WhenNoProjects()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projects = await repository.GetProjectsByDirectorIdAsync(Guid.NewGuid());

        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetProjectsByEmployeeIdAsync_ShouldReturnProjectsForEmployee()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projects = await repository.GetProjectsByEmployeeIdAsync(_employeeId1);

        Assert.Equal(2, projects.Count);
        var projectIds = projects.Select(p => p.Id).ToHashSet();
        Assert.Contains(_projectId1, projectIds);
        Assert.Contains(_projectId2, projectIds);
    }

    [Fact]
    public async Task GetProjectsByEmployeeIdAsync_ShouldReturnEmpty_WhenNoProjects()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projects = await repository.GetProjectsByEmployeeIdAsync(Guid.NewGuid());

        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetProjectIdsByEmployeeIdAsync_ShouldReturnProjectIds()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projectIds = await repository.GetProjectIdsByEmployeeIdAsync(_employeeId1);

        Assert.Equal(2, projectIds.Count);
        Assert.Contains(_projectId1, projectIds);
        Assert.Contains(_projectId2, projectIds);
    }

    [Fact]
    public async Task GetProjectIdsByEmployeeIdAsync_ShouldReturnEmpty_WhenNoProjects()
    {
        await using var context = new AppDbContext(_options);
        var repository = new ProjectRepository(context);

        var projectIds = await repository.GetProjectIdsByEmployeeIdAsync(Guid.NewGuid());

        Assert.Empty(projectIds);
    }
}