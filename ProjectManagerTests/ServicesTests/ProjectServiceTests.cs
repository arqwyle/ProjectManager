using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockEmployeeRepo = new Mock<IEmployeeRepository>();
        _service = new ProjectService(_mockProjectRepo.Object, _mockEmployeeRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithDefaultParameters()
    {
        var projects = new List<Project>();
        _mockProjectRepo.Setup(r => r.GetAllAsync())
                 .ReturnsAsync(projects);

        var result = await _service.GetAllAsync();

        Assert.Equal(projects, result);
        _mockProjectRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithFilters()
    {
        var customersName = "TestCustomers";
        var executorName = "TestExecutor";
        var startTimeFrom = DateTime.Now;
        var startTimeTo = DateTime.Now.AddDays(1);
        var priorities = new List<int> { 1, 2 };
        var projects = new List<Project>();

        _mockProjectRepo.Setup(r => r.GetAllAsync(customersName, executorName, startTimeFrom, startTimeTo, priorities, "Name", false))
                 .ReturnsAsync(projects);

        var result = await _service.GetAllAsync(customersName, executorName, startTimeFrom, startTimeTo, priorities, "Name", false);

        Assert.Equal(projects, result);
        _mockProjectRepo.Verify(r => r.GetAllAsync(customersName, executorName, startTimeFrom, startTimeTo, priorities, "Name", false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(project);

        var result = await _service.GetByIdAsync(id);

        Assert.Equal(project, result);
        _mockProjectRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };
        _mockProjectRepo.Setup(r => r.AddAsync(project)).ReturnsAsync(project);

        var result = await _service.AddAsync(project);

        Assert.Equal(project, result);
        _mockProjectRepo.Verify(r => r.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1
        };
        _mockProjectRepo.Setup(r => r.UpdateAsync(project)).Returns(Task.CompletedTask);

        await _service.UpdateAsync(project);

        _mockProjectRepo.Verify(r => r.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _mockProjectRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(id);

        _mockProjectRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockProjectRepo.Setup(r => r.AddEmployeeToProjectAsync(projectId, employeeId))
                 .Returns(Task.CompletedTask);

        await _service.AddEmployeeToProjectAsync(projectId, employeeId);

        _mockProjectRepo.Verify(r => r.AddEmployeeToProjectAsync(projectId, employeeId), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeLinksAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _mockProjectRepo.Setup(r => r.UpdateEmployeeLinksAsync(projectId, employeeIds))
                 .Returns(Task.CompletedTask);

        await _service.UpdateEmployeeLinksAsync(projectId, employeeIds);

        _mockProjectRepo.Verify(r => r.UpdateEmployeeLinksAsync(projectId, employeeIds), Times.Once);
    }
    
    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockProjectRepo.Setup(r => r.RemoveEmployeeFromProjectAsync(projectId, employeeId))
            .Returns(Task.CompletedTask);

        await _service.RemoveEmployeeFromProjectAsync(projectId, employeeId);

        _mockProjectRepo.Verify(r => r.RemoveEmployeeFromProjectAsync(projectId, employeeId), Times.Once);
    }
    
    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockProjectRepo.Setup(r => r.AddObjectiveToProjectAsync(projectId, objectiveId))
            .Returns(Task.CompletedTask);

        await _service.AddObjectiveToProjectAsync(projectId, objectiveId);

        _mockProjectRepo.Verify(r => r.AddObjectiveToProjectAsync(projectId, objectiveId), Times.Once);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockProjectRepo.Setup(r => r.RemoveObjectiveFromProjectAsync(projectId, objectiveId))
            .Returns(Task.CompletedTask);

        await _service.RemoveObjectiveFromProjectAsync(projectId, objectiveId);

        _mockProjectRepo.Verify(r => r.RemoveObjectiveFromProjectAsync(projectId, objectiveId), Times.Once);
    }
    
    [Fact]
    public async Task GetManagerProjectsAsync_ShouldReturnEmptyList_WhenEmployeeNotFound()
    {
        var userId = "unknown";
        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetManagerProjectsAsync(userId);

        Assert.Empty(result);
        _mockProjectRepo.Verify(r => r.GetProjectsByDirectorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetManagerProjectsAsync_ShouldReturnProjects_WhenEmployeeExists()
    {
        var userId = "manager";
        var employeeId = Guid.NewGuid();
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                Name = "Test", 
                CustomerName = "Test", 
                ExecutorName = "Test", 
                Priority = 1
            }
        };

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockProjectRepo.Setup(r => r.GetProjectsByDirectorIdAsync(employeeId)).ReturnsAsync(projects);

        var result = await _service.GetManagerProjectsAsync(userId);

        Assert.Equal(projects, result);
        _mockProjectRepo.Verify(r => r.GetProjectsByDirectorIdAsync(employeeId), Times.Once);
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

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockEmployeeRepo.Setup(r => r.GetProjectsByEmployeeIdAsync(employeeId)).ReturnsAsync(projects);

        var result = await _service.GetEmployeeProjectsAsync(userId);

        Assert.Equal(projects, result);
    }

    [Fact]
    public async Task GetEmployeeProjectsAsync_ShouldReturnEmptyList_WhenEmployeeNotExists()
    {
        var userId = "Test";

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetEmployeeProjectsAsync(userId);

        Assert.Empty(result);
    }
}