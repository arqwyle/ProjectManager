using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepo;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepository()
    {
        var employees = new List<Employee> { new()
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

        var result = await _service.GetAllAsync();

        Assert.Equal(employees, result);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

        var result = await _service.GetByIdAsync(id);

        Assert.Equal(employee, result);
        _mockRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        await _service.AddAsync(employee);
        _mockRepo.Verify(r => r.AddAsync(employee), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        await _service.UpdateAsync(employee);
        _mockRepo.Verify(r => r.UpdateAsync(employee), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        await _service.DeleteAsync(id);
        _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
    
    [Fact]
    public async Task GetEmployeeProjectsAsync_ShouldReturnProjects_WhenEmployeeExists()
    {
        var userId = "Test";
        var employeeId = Guid.NewGuid();
        var projects = new List<Project> { new()
        {
            Id = Guid.NewGuid(), 
            Name = "1", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        } };

        _mockRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockRepo.Setup(r => r.GetProjectsByEmployeeIdAsync(employeeId)).ReturnsAsync(projects);

        var result = await _service.GetEmployeeProjectsAsync(userId);

        Assert.Equal(projects, result);
    }

    [Fact]
    public async Task GetEmployeeProjectsAsync_ShouldReturnEmptyList_WhenEmployeeNotExists()
    {
        var userId = "Test";

        _mockRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetEmployeeProjectsAsync(userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEmployeeObjectivesAsync_ShouldReturnObjectives_WhenEmployeeExists()
    {
        var userId = "Test";
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective> { new()
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        }};

        _mockRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockRepo.Setup(r => r.GetObjectivesByEmployeeIdAsync(employeeId)).ReturnsAsync(objectives);

        var result = await _service.GetEmployeeObjectivesAsync(userId);

        Assert.Equal(objectives, result);
    }

    [Fact]
    public async Task GetEmployeeObjectivesAsync_ShouldReturnEmptyList_WhenEmployeeNotExists()
    {
        var userId = "Test";

        _mockRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetEmployeeObjectivesAsync(userId);

        Assert.Empty(result);
    }
}