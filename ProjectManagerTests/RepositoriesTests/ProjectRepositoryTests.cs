using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class ProjectRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly ProjectRepository _repository;

    public ProjectRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new ProjectRepository(_context);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProjects()
    {
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                Name = "1", 
                CustomerName = "Test", 
                ExecutorName = "Test", 
                Priority = 1
            },
            new()
            {
                Id = Guid.NewGuid(), 
                Name = "2", 
                CustomerName = "Test", 
                ExecutorName = "Test", 
                Priority = 1
            }
        };
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "1");
        Assert.Contains(result, p => p.Name == "2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProjectWithEmployeeLinks()
    {
        var projectId = Guid.NewGuid();
        var empId1 = Guid.NewGuid();
        var empId2 = Guid.NewGuid();

        _context.Projects.Add(new Project
        {
            Id = projectId, 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        });
        _context.Employees.AddRange(
            new Employee
            {
                Id = empId1, 
                FirstName = "1", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            },
            new Employee
            {
                Id = empId2, 
                FirstName = "2", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            }
        );
        _context.EmployeeProjects.AddRange(
            new EmployeeProject { ProjectId = projectId, EmployeeId = empId1 },
            new EmployeeProject { ProjectId = projectId, EmployeeId = empId2 }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(projectId);

        Assert.NotNull(result);
        Assert.Equal(2, result.EmployeeProjects.Count);
        Assert.Contains(result.EmployeeProjects, ep => ep.EmployeeId == empId1);
        Assert.Contains(result.EmployeeProjects, ep => ep.EmployeeId == empId2);
    }
    
    [Fact]
    public async Task AddAsync_ShouldAddProject()
    {
        var project = new Project 
        { 
            Id = Guid.NewGuid(), 
            Name = "New", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };

        await _repository.AddAsync(project);
        await _context.SaveChangesAsync();

        var saved = await _context.Projects.FindAsync(project.Id);
        Assert.NotNull(saved);
        Assert.Equal("New", saved.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProject()
    {
        var project = new Project 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        project.Name = "Updated Name";
        project.CustomerName = "Updated Customer";
        await _repository.UpdateAsync(project);
        await _context.SaveChangesAsync();

        var updated = await _context.Projects.FindAsync(project.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Customer", updated.CustomerName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProjectAndEmployeeLinks()
    {
        var projectId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        
        _context.Projects.Add(new Project
        {
            Id = projectId, 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        });
        _context.Employees.Add(new Employee
        {
            Id = empId, 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        });
        _context.EmployeeProjects.Add(new EmployeeProject { ProjectId = projectId, EmployeeId = empId });
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(projectId);
        await _context.SaveChangesAsync();

        var project = await _context.Projects.FindAsync(projectId);
        var employeeLink = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId);
        
        Assert.Null(project);
        Assert.False(employeeLink);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenProjectNotFound()
    {
        await _repository.DeleteAsync(Guid.NewGuid());
        await _context.SaveChangesAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldAddLink()
    {
        var projectId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        _context.Projects.Add(new Project
        {
            Id = projectId, 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        });
        _context.Employees.Add(new Employee
        {
            Id = empId, 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        });
        await _context.SaveChangesAsync();

        await _repository.AddEmployeeToProjectAsync(projectId, empId);
        await _context.SaveChangesAsync();

        var linkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == empId);
        Assert.True(linkExists);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldAddLink_WhenNotExists()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _repository.AddEmployeeToProjectAsync(projectId, employeeId);
        await _context.SaveChangesAsync();

        var linkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
        Assert.True(linkExists);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldNotDuplicateLink()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _repository.AddEmployeeToProjectAsync(projectId, employeeId);
        await _repository.AddEmployeeToProjectAsync(projectId, employeeId);
        await _context.SaveChangesAsync();

        var count = await _context.EmployeeProjects
            .CountAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpdateEmployeeLinksAsync_ShouldReplaceAllLinks()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var emp1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var emp2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await _repository.AddEmployeeToProjectAsync(projectId, emp1);
        await _context.SaveChangesAsync();

        await _repository.UpdateEmployeeLinksAsync(projectId, [emp2]);
        await _context.SaveChangesAsync();

        var links = await _context.EmployeeProjects
            .Where(ep => ep.ProjectId == projectId)
            .Select(ep => ep.EmployeeId)
            .ToListAsync();

        Assert.Single(links);
        Assert.Contains(emp2, links);
        Assert.DoesNotContain(emp1, links);
    }
    
    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldRemoveLink_WhenLinkExists()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _context.EmployeeProjects.AddAsync(new EmployeeProject
        {
            ProjectId = projectId,
            EmployeeId = employeeId
        });
        await _context.SaveChangesAsync();

        await _repository.RemoveEmployeeFromProjectAsync(projectId, employeeId);
        await _context.SaveChangesAsync();

        var linkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
        Assert.False(linkExists);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _repository.RemoveEmployeeFromProjectAsync(projectId, employeeId);
        await _context.SaveChangesAsync();

        var linkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
        Assert.False(linkExists);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldNotAffectOtherLinks()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var employeeId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await _context.EmployeeProjects.AddRangeAsync(
            new EmployeeProject { ProjectId = projectId, EmployeeId = employeeId1 },
            new EmployeeProject { ProjectId = projectId, EmployeeId = employeeId2 }
        );
        await _context.SaveChangesAsync();

        await _repository.RemoveEmployeeFromProjectAsync(projectId, employeeId1);
        await _context.SaveChangesAsync();

        var remainingLinkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId2);
        Assert.True(remainingLinkExists);

        var removedLinkExists = await _context.EmployeeProjects
            .AnyAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId1);
        Assert.False(removedLinkExists);
    }
    
    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldUpdateObjectiveProjectId()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var objectiveId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            AuthorId = Guid.NewGuid(),
            Comment = "Test",
            Priority = 1,
            Status = Status.ToDo,
            ProjectId = Guid.NewGuid()
        };
        
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        await _repository.AddObjectiveToProjectAsync(projectId, objectiveId);
        await _context.SaveChangesAsync();

        var updatedObjective = await _context.Objectives.FindAsync(objectiveId);
        Assert.NotNull(updatedObjective);
        Assert.Equal(projectId, updatedObjective.ProjectId);
    }

    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldDoNothing_WhenObjectiveDoesNotExist()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var objectiveId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _repository.AddObjectiveToProjectAsync(projectId, objectiveId);
        await _context.SaveChangesAsync();

        var objectiveExists = await _context.Objectives.AnyAsync(o => o.Id == objectiveId);
        Assert.False(objectiveExists);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldRemoveObjective_WhenExistsInProject()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var objectiveId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            AuthorId = Guid.NewGuid(),
            Comment = "Test",
            Priority = 1,
            Status = Status.ToDo,
            ProjectId = projectId
        };
        
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        await _repository.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
        await _context.SaveChangesAsync();

        var objectiveExists = await _context.Objectives.AnyAsync(o => o.Id == objectiveId);
        Assert.False(objectiveExists);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldDoNothing_WhenObjectiveDoesNotExist()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var objectiveId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await _repository.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
        await _context.SaveChangesAsync();

        var objectiveExists = await _context.Objectives.AnyAsync(o => o.Id == objectiveId);
        Assert.False(objectiveExists);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldDoNothing_WhenObjectiveInDifferentProject()
    {
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var otherProjectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var objectiveId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        var objective = new Objective
        {
            Id = objectiveId,
            Name = "Test",
            AuthorId = Guid.NewGuid(),
            Comment = "Test",
            Priority = 1,
            Status = Status.ToDo,
            ProjectId = otherProjectId
        };
        
        await _context.Objectives.AddAsync(objective);
        await _context.SaveChangesAsync();

        await _repository.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
        await _context.SaveChangesAsync();

        var objectiveExists = await _context.Objectives.AnyAsync(o => o.Id == objectiveId);
        Assert.True(objectiveExists);
    }
    
    [Fact]
    public async Task GetProjectsByDirectorIdAsync_ShouldReturnProjects_WhenExists()
    {
        var directorId = Guid.NewGuid();
        var project1 = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1, 
            DirectorId = directorId
        };
        var project2 = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1, 
            DirectorId = directorId
        };
        var otherProject = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };

        await _context.Projects.AddRangeAsync(project1, project2, otherProject);
        await _context.SaveChangesAsync();

        var result = await _repository.GetProjectsByDirectorIdAsync(directorId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == project1.Id);
        Assert.Contains(result, p => p.Id == project2.Id);
        Assert.DoesNotContain(result, p => p.Id == otherProject.Id);
    }

    [Fact]
    public async Task GetProjectsByDirectorIdAsync_ShouldReturnEmptyList_WhenNoProjects()
    {
        var result = await _repository.GetProjectsByDirectorIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }
}