using Microsoft.EntityFrameworkCore;
using ProjectManager.Database;
using ProjectManager.Models;
using ProjectManager.Repositories;

namespace ProjectManagerTests.RepositoriesTests;

public class EmployeeRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly EmployeeRepository _repository;

    public EmployeeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new EmployeeRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                FirstName = "1", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(), 
                FirstName = "2", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            }
        };
        _context.Employees.AddRange(employees);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.FirstName == "1");
        Assert.Contains(result, e => e.FirstName == "2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WhenExists()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal("Test", result.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEmployee()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };

        await _repository.AddAsync(employee);
        await _context.SaveChangesAsync();

        var saved = await _context.Employees.FindAsync(employee.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test", saved.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployee()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "1", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        employee.FirstName = "2";
        await _repository.UpdateAsync(employee);
        await _context.SaveChangesAsync();

        var updated = await _context.Employees.FindAsync(employee.Id);
        Assert.NotNull(updated);
        Assert.Equal("2", updated.FirstName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEmployee()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        var id = employee.Id;

        await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();

        var deleted = await _context.Employees.FindAsync(id);
        Assert.Null(deleted);
    }
    
    [Fact]
    public async Task GetEmployeeIdByNameAsync_ShouldReturnEmployeeId_WhenExists()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic = "Test", 
            Mail = "Test",
            UserId = "Test"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var result = await _repository.GetEmployeeIdByUserIdAsync("Test");

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result);
    }

    [Fact]
    public async Task GetEmployeeIdByNameAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetEmployeeIdByUserIdAsync("NonExistent");

        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public async Task GetProjectsByEmployeeIdAsync_ShouldReturnProjects_WhenEmployeeHasProjects()
    {
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var project = new Project 
        { 
            Id = projectId, 
            Name = "Project1", 
            CustomerName = "Customer1", 
            ExecutorName = "Executor1", 
            StartTime = DateTime.Now, 
            EndTime = DateTime.Now.AddDays(7), 
            Priority = 1, 
            DirectorId = Guid.NewGuid() 
        };
        var employeeProject = new EmployeeProject { EmployeeId = employeeId, ProjectId = projectId };

        _context.Projects.Add(project);
        _context.EmployeeProjects.Add(employeeProject);
        await _context.SaveChangesAsync();

        var result = await _repository.GetProjectsByEmployeeIdAsync(employeeId);

        Assert.Single(result);
        Assert.Equal("Project1", result[0].Name);
    }

    [Fact]
    public async Task GetProjectsByEmployeeIdAsync_ShouldReturnEmptyList_WhenEmployeeHasNoProjects()
    {
        var result = await _repository.GetProjectsByEmployeeIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetObjectivesByEmployeeIdAsync_ShouldReturnObjectives_WhenEmployeeHasObjectives()
    {
        var employeeId = Guid.NewGuid();
        var objective = new Objective 
        { 
            Id = Guid.NewGuid(), 
            Name = "Objective1", 
            AuthorId = Guid.NewGuid(), 
            ExecutorId = employeeId,
            Status = Status.ToDo, 
            Comment = "Comment1", 
            Priority = 1, 
            ProjectId = Guid.NewGuid() 
        };

        _context.Objectives.Add(objective);
        await _context.SaveChangesAsync();

        var result = await _repository.GetObjectivesByEmployeeIdAsync(employeeId);

        Assert.Single(result);
        Assert.Equal("Objective1", result[0].Name);
        Assert.Equal(employeeId, result[0].ExecutorId);
    }

    [Fact]
    public async Task GetObjectivesByEmployeeIdAsync_ShouldReturnEmptyList_WhenEmployeeHasNoObjectives()
    {
        var result = await _repository.GetObjectivesByEmployeeIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }
}