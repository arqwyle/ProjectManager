using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class ObjectiveServiceTests
{
    private readonly Mock<IObjectiveRepository> _mockRepo;
    private readonly ObjectiveService _service;

    public ObjectiveServiceTests()
    {
        _mockRepo = new Mock<IObjectiveRepository>();
        _service = new ObjectiveService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepository()
    {
        var objectives = new List<Objective>();
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(objectives);

        var result = await _service.GetAllAsync();

        Assert.Same(objectives, result);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithFilters()
    {   
        var statuses = new List<Status> { Status.ToDo, Status.InProgress };
        var priorities = new List<int> { 1, 2 };
        var objectives = new List<Objective>();

        _mockRepo.Setup(r => r.GetAllAsync(statuses, priorities, "Name", false))
            .ReturnsAsync(objectives);

        var result = await _service.GetAllAsync(statuses, priorities, "Name", false);

        Assert.Equal(objectives, result);
        _mockRepo.Verify(r => r.GetAllAsync(statuses, priorities, "Name", false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var objective = new Objective{Name = "Test", Priority = 1};
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(objective);

        var result = await _service.GetByIdAsync(id);

        Assert.Same(objective, result);
        _mockRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var objective = new Objective{Name = "Test", Priority = 1};
        _mockRepo.Setup(r => r.AddAsync(objective)).Returns(Task.CompletedTask);

        await _service.AddAsync(objective);

        _mockRepo.Verify(r => r.AddAsync(objective), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var objective = new Objective{Name = "Test", Priority = 1};
        _mockRepo.Setup(r => r.UpdateAsync(objective)).Returns(Task.CompletedTask);

        await _service.UpdateAsync(objective);

        _mockRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
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
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldCallRepository()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepo.Setup(r => r.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(true);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.True(result);
        _mockRepo.Verify(r => r.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId), Times.Once);
    }
}