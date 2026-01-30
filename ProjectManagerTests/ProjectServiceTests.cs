using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockRepo;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _mockRepo = new Mock<IProjectRepository>();
        _service = new ProjectService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithDefaultParameters()
    {
        var projects = new List<Project>();
        _mockRepo.Setup(r => r.GetAllAsync(null, null, null, null, true))
                 .ReturnsAsync(projects);

        var result = await _service.GetAllAsync();

        Assert.Equal(projects, result);
        _mockRepo.Verify(r => r.GetAllAsync(null, null, null, null, true), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithFilters()
    {
        var startTimeFrom = DateTime.Now;
        var startTimeTo = DateTime.Now.AddDays(1);
        var priorities = new List<int> { 1, 2 };
        var projects = new List<Project>();

        _mockRepo.Setup(r => r.GetAllAsync(startTimeFrom, startTimeTo, priorities, "Name", false))
                 .ReturnsAsync(projects);

        var result = await _service.GetAllAsync(startTimeFrom, startTimeTo, priorities, "Name", false);

        Assert.Equal(projects, result);
        _mockRepo.Verify(r => r.GetAllAsync(startTimeFrom, startTimeTo, priorities, "Name", false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var project = new Project { Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(project);

        var result = await _service.GetByIdAsync(id);

        Assert.Equal(project, result);
        _mockRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var project = new Project { Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockRepo.Setup(r => r.AddAsync(project)).ReturnsAsync(project);

        var result = await _service.AddAsync(project);

        Assert.Equal(project, result);
        _mockRepo.Verify(r => r.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockRepo.Setup(r => r.UpdateAsync(project)).Returns(Task.CompletedTask);

        await _service.UpdateAsync(project);

        _mockRepo.Verify(r => r.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(id);

        _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepo.Setup(r => r.AddEmployeeToProjectAsync(projectId, employeeId))
                 .Returns(Task.CompletedTask);

        await _service.AddEmployeeToProjectAsync(projectId, employeeId);

        _mockRepo.Verify(r => r.AddEmployeeToProjectAsync(projectId, employeeId), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeLinksAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _mockRepo.Setup(r => r.UpdateEmployeeLinksAsync(projectId, employeeIds))
                 .Returns(Task.CompletedTask);

        await _service.UpdateEmployeeLinksAsync(projectId, employeeIds);

        _mockRepo.Verify(r => r.UpdateEmployeeLinksAsync(projectId, employeeIds), Times.Once);
    }
    
    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepo.Setup(r => r.RemoveEmployeeFromProjectAsync(projectId, employeeId))
            .Returns(Task.CompletedTask);

        await _service.RemoveEmployeeFromProjectAsync(projectId, employeeId);

        _mockRepo.Verify(r => r.RemoveEmployeeFromProjectAsync(projectId, employeeId), Times.Once);
    }
}