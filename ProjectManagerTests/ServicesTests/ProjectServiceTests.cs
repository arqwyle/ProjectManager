using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockRepository;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _mockRepository = new Mock<IProjectRepository>();
        _service = new ProjectService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithParameters()
    {
        var customerName = "Test";
        var executorName = "Test";
        var startTimeFrom = DateTime.Now;
        var startTimeTo = DateTime.Now.AddDays(1);
        var priorities = new List<int> { 1, 2 };
        var sortBy = "name";
        var isAsc = true;
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = Guid.NewGuid() }
        };
        _mockRepository.Setup(r => r.GetAllAsync(customerName, executorName, startTimeFrom, startTimeTo, priorities, sortBy, isAsc))
            .ReturnsAsync(projects);

        var result = await _service.GetAllAsync(customerName, executorName, startTimeFrom, startTimeTo, priorities, sortBy, isAsc);

        Assert.Single(result);
        _mockRepository.Verify(r => r.GetAllAsync(customerName, executorName, startTimeFrom, startTimeTo, priorities, sortBy, isAsc), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProjectFromRepository()
    {
        var id = Guid.NewGuid();
        var project = new Project { Id = id, Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(project);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        var result = await _service.GetByIdAsync(id);

        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepositoryAddAsync()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = Guid.NewGuid() };

        await _service.AddAsync(project);

        _mockRepository.Verify(r => r.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = Guid.NewGuid() };

        await _service.UpdateAsync(project);

        _mockRepository.Verify(r => r.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
    {
        var id = Guid.NewGuid();

        await _service.DeleteAsync(id);

        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddEmployeeToProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        await _service.AddEmployeeToProjectAsync(projectId, employeeId);

        _mockRepository.Verify(r => r.AddEmployeeToProjectAsync(projectId, employeeId), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeLinksAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        await _service.UpdateEmployeeLinksAsync(projectId, employeeIds);

        _mockRepository.Verify(r => r.UpdateEmployeeLinksAsync(projectId, employeeIds), Times.Once);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        await _service.RemoveEmployeeFromProjectAsync(projectId, employeeId);

        _mockRepository.Verify(r => r.RemoveEmployeeFromProjectAsync(projectId, employeeId), Times.Once);
    }

    [Fact]
    public async Task AddObjectiveToProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();

        await _service.AddObjectiveToProjectAsync(projectId, objectiveId);

        _mockRepository.Verify(r => r.AddObjectiveToProjectAsync(projectId, objectiveId), Times.Once);
    }

    [Fact]
    public async Task RemoveObjectiveFromProjectAsync_ShouldCallRepository()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();

        await _service.RemoveObjectiveFromProjectAsync(projectId, objectiveId);

        _mockRepository.Verify(r => r.RemoveObjectiveFromProjectAsync(projectId, objectiveId), Times.Once);
    }

    [Fact]
    public async Task GetManagerProjectsAsync_ShouldCallRepository()
    {
        var employeeId = Guid.NewGuid();
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = employeeId }
        };
        _mockRepository.Setup(r => r.GetProjectsByDirectorIdAsync(employeeId))
            .ReturnsAsync(projects);

        var result = await _service.GetManagerProjectsAsync(employeeId);

        Assert.Single(result);
        _mockRepository.Verify(r => r.GetProjectsByDirectorIdAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeProjectsAsync_ShouldCallRepository()
    {
        var employeeId = Guid.NewGuid();
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1, DirectorId = Guid.NewGuid() }
        };
        _mockRepository.Setup(r => r.GetProjectsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(projects);

        var result = await _service.GetEmployeeProjectsAsync(employeeId);

        Assert.Single(result);
        _mockRepository.Verify(r => r.GetProjectsByEmployeeIdAsync(employeeId), Times.Once);
    }
}