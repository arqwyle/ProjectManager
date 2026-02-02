using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class EmployeeRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Guid _employeeId;
    private readonly Guid _projectId;
    private readonly Guid _authoredObjectiveId;
    private readonly Guid _assignedObjectiveId;

    public EmployeeRepositoryTests()
    {
        _employeeId = Guid.NewGuid();
        _projectId = Guid.NewGuid();
        _authoredObjectiveId = Guid.NewGuid();
        _assignedObjectiveId = Guid.NewGuid();

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

        var employee = new Employee
        {
            Id = _employeeId,
            UserId = "1",
            FirstName = "Test",
            LastName = "Test",
            Patronymic = "Test",
            Mail = "Test"
        };

        var project = new Project
        {
            Id = _projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        var authoredObjective = new Objective
        {
            Id = _authoredObjectiveId,
            Name = "Test",
            Priority = 1,
            AuthorId = _employeeId
        };

        var assignedObjective = new Objective
        {
            Id = _assignedObjectiveId,
            Name = "Test",
            Priority = 1,
            ExecutorId = _employeeId
        };

        context.Employees.Add(employee);
        context.Projects.Add(project);
        context.Objectives.AddRange(authoredObjective, assignedObjective);

        context.EmployeeProjects.Add(new EmployeeProject
        {
            EmployeeId = _employeeId,
            ProjectId = _projectId
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WithIncludes()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);

        var employee = await repository.GetByIdAsync(_employeeId);

        Assert.NotNull(employee);
        Assert.Equal(_employeeId, employee.Id);
        Assert.Equal("1", employee.UserId);
        Assert.Equal("Test", employee.FirstName);
        Assert.Equal("Test", employee.LastName);
        Assert.Equal("Test", employee.Patronymic);
        Assert.Equal("Test", employee.Mail);
        Assert.Single(employee.EmployeeProjects);
        Assert.Equal(_projectId, employee.EmployeeProjects.First().ProjectId);
        Assert.Single(employee.AuthoredObjectives);
        Assert.Equal(_authoredObjectiveId, employee.AuthoredObjectives.First().Id);
        Assert.Single(employee.AssignedObjectives);
        Assert.Equal(_assignedObjectiveId, employee.AssignedObjectives.First().Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEmployeeNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);
        var nonExistentId = Guid.NewGuid();

        var employee = await repository.GetByIdAsync(nonExistentId);

        Assert.Null(employee);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees_WithIncludes()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);

        var employees = await repository.GetAllAsync();

        Assert.Single(employees);
        var employee = employees[0];
        Assert.Equal(_employeeId, employee.Id);
        Assert.Single(employee.EmployeeProjects);
        Assert.Single(employee.AuthoredObjectives);
        Assert.Single(employee.AssignedObjectives);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEmployeeToDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);
        var newEmployeeId = Guid.NewGuid();
        var newEmployee = new Employee
        {
            Id = newEmployeeId,
            UserId = "2",
            FirstName = "Test",
            LastName = "Test",
            Patronymic = "Test",
            Mail = "Test"
        };

        await repository.AddAsync(newEmployee);
        var savedEmployee = await context.Employees.FindAsync(newEmployeeId);

        Assert.NotNull(savedEmployee);
        Assert.Equal(newEmployeeId, savedEmployee.Id);
        Assert.Equal("2", savedEmployee.UserId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployeeInDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);
        var employee = await context.Employees.FindAsync(_employeeId);
        Assert.NotNull(employee);
        employee.FirstName = "Updated";

        await repository.UpdateAsync(employee);
        var updatedEmployee = await context.Employees.FindAsync(_employeeId);

        Assert.NotNull(updatedEmployee);
        Assert.Equal("Updated", updatedEmployee.FirstName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEmployeeFromDatabase()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);

        await repository.DeleteAsync(_employeeId);
        var deletedEmployee = await context.Employees.FindAsync(_employeeId);

        Assert.Null(deletedEmployee);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenEmployeeNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);
        var nonExistentId = Guid.NewGuid();

        await repository.DeleteAsync(nonExistentId);
    }

    [Fact]
    public async Task GetEmployeeIdByUserIdAsync_ShouldReturnId_WhenUserExists()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);

        var employeeId = await repository.GetEmployeeIdByUserIdAsync("1");

        Assert.Equal(_employeeId, employeeId);
    }

    [Fact]
    public async Task GetEmployeeIdByUserIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        await using var context = new AppDbContext(_options);
        var repository = new EmployeeRepository(context);

        var employeeId = await repository.GetEmployeeIdByUserIdAsync("nonexistent");

        Assert.Equal(Guid.Empty, employeeId);
    }
}