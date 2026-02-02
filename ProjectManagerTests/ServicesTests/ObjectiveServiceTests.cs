using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class ObjectiveServiceTests
{
    private readonly Mock<IObjectiveRepository> _mockObjectiveRepo;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly ObjectiveService _service;

    public ObjectiveServiceTests()
    {
        _mockObjectiveRepo = new Mock<IObjectiveRepository>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _service = new ObjectiveService(_mockObjectiveRepo.Object, _mockProjectRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithParameters()
    {
        var statuses = new List<Status> { Status.ToDo };
        var priorities = new List<int> { 1 };
        var sortBy = "name";
        var isAsc = true;
        var objectives = new List<Objective>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", Priority = 1, Status = Status.ToDo }
        };
        _mockObjectiveRepo.Setup(r => r.GetAllAsync(statuses, priorities, sortBy, isAsc))
            .ReturnsAsync(objectives);

        var result = await _service.GetAllAsync(statuses, priorities, sortBy, isAsc);

        Assert.Single(result);
        _mockObjectiveRepo.Verify(r => r.GetAllAsync(statuses, priorities, sortBy, isAsc), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnObjectiveFromRepository()
    {
        var id = Guid.NewGuid();
        var objective = new Objective { Id = id, Name = "Test", Priority = 1, Status = Status.ToDo };
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(objective);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockObjectiveRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var id = Guid.NewGuid();
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Objective?)null);

        var result = await _service.GetByIdAsync(id);

        Assert.Null(result);
        _mockObjectiveRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepositoryAddAsync()
    {
        var objective = new Objective { Id = Guid.NewGuid(), Name = "Test", Priority = 1, Status = Status.ToDo };

        await _service.AddAsync(objective);

        _mockObjectiveRepo.Verify(r => r.AddAsync(objective), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
    {
        var objective = new Objective { Id = Guid.NewGuid(), Name = "Test", Priority = 1, Status = Status.ToDo };

        await _service.UpdateAsync(objective);

        _mockObjectiveRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
    {
        var id = Guid.NewGuid();

        await _service.DeleteAsync(id);

        _mockObjectiveRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeObjectivesAsync_ShouldCallRepository()
    {
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", Priority = 1, Status = Status.ToDo, ExecutorId = employeeId }
        };
        _mockObjectiveRepo.Setup(r => r.GetObjectivesByEmployeeIdAsync(employeeId))
            .ReturnsAsync(objectives);

        var result = await _service.GetEmployeeObjectivesAsync(employeeId);

        Assert.Single(result);
        _mockObjectiveRepo.Verify(r => r.GetObjectivesByEmployeeIdAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjectsAsync_ShouldCallRepository()
    {
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", Priority = 1, Status = Status.ToDo }
        };
        _mockObjectiveRepo.Setup(r => r.GetObjectivesByDirectorIdAsync(employeeId))
            .ReturnsAsync(objectives);

        var result = await _service.GetObjectivesForManagerProjectsAsync(employeeId);

        Assert.Single(result);
        _mockObjectiveRepo.Verify(r => r.GetObjectivesByDirectorIdAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnTrue_WhenEmployeeIsInProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId};
        var projectIds = new List<Guid> { projectId };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectRepo.Setup(r => r.GetProjectIdsByEmployeeIdAsync(employeeId)).ReturnsAsync(projectIds);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnFalse_WhenObjectiveNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnFalse_WhenObjectiveHasNoProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ProjectId = null};
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldReturnFalse_WhenEmployeeNotInProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId};
        var projectIds = new List<Guid> { Guid.NewGuid() };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectRepo.Setup(r => r.GetProjectIdsByEmployeeIdAsync(employeeId)).ReturnsAsync(projectIds);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnTrue_WhenDirector()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo };
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, true);

        Assert.True(result);
        _mockObjectiveRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
        Assert.Equal(Status.Done, objective.Status);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnTrue_WhenExecutor()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ExecutorId = employeeId };
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false);

        Assert.True(result);
        _mockObjectiveRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
        Assert.Equal(Status.Done, objective.Status);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnTrue_WhenProjectManager()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId };
        var project = new Project { Id = projectId, DirectorId = employeeId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1 };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false);

        Assert.True(result);
        _mockObjectiveRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
        Assert.Equal(Status.Done, objective.Status);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenObjectiveNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenNotAuthorized()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ExecutorId = otherEmployeeId, ProjectId = projectId };
        var project = new Project { Id = projectId, DirectorId = otherEmployeeId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(1), Priority = 1 };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false);

        Assert.False(result);
        _mockObjectiveRepo.Verify(r => r.UpdateAsync(It.IsAny<Objective>()), Times.Never);
    }

    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenProjectNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1, Status = Status.ToDo, ProjectId = projectId };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false);

        Assert.False(result);
    }
}