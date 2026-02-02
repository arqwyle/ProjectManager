using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class ObjectiveServiceTests
{
    private readonly Mock<IObjectiveRepository> _mockObjectiveRepo;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly ObjectiveService _service;

    public ObjectiveServiceTests()
    {
        _mockObjectiveRepo = new Mock<IObjectiveRepository>();
        _mockEmployeeRepo = new Mock<IEmployeeRepository>();
        _mockProjectRepo = new  Mock<IProjectRepository>();
        _service = new ObjectiveService(_mockObjectiveRepo.Object, _mockEmployeeRepo.Object, _mockProjectRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepository()
    {
        var objectives = new List<Objective>();
        _mockObjectiveRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(objectives);

        var result = await _service.GetAllAsync();

        Assert.Same(objectives, result);
        _mockObjectiveRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldCallRepositoryWithFilters()
    {   
        var statuses = new List<Status> { Status.ToDo, Status.InProgress };
        var priorities = new List<int> { 1, 2 };
        var objectives = new List<Objective>();

        _mockObjectiveRepo.Setup(r => r.GetAllAsync(statuses, priorities, "Name", false))
            .ReturnsAsync(objectives);

        var result = await _service.GetAllAsync(statuses, priorities, "Name", false);

        Assert.Equal(objectives, result);
        _mockObjectiveRepo.Verify(r => r.GetAllAsync(statuses, priorities, "Name", false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var objective = new Objective{Id = Guid.NewGuid(), Name = "Test", Priority = 1};
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(objective);

        var result = await _service.GetByIdAsync(id);

        Assert.Same(objective, result);
        _mockObjectiveRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var objective = new Objective{Id = Guid.NewGuid(), Name = "Test", Priority = 1};
        _mockObjectiveRepo.Setup(r => r.AddAsync(objective)).Returns(Task.CompletedTask);

        await _service.AddAsync(objective);

        _mockObjectiveRepo.Verify(r => r.AddAsync(objective), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var objective = new Objective{Id = Guid.NewGuid(), Name = "Test", Priority = 1};
        _mockObjectiveRepo.Setup(r => r.UpdateAsync(objective)).Returns(Task.CompletedTask);

        await _service.UpdateAsync(objective);

        _mockObjectiveRepo.Verify(r => r.UpdateAsync(objective), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _mockObjectiveRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(id);

        _mockObjectiveRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task IsEmployeeInObjectiveProjectAsync_ShouldCallRepository()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockObjectiveRepo.Setup(r => r.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(true);

        var result = await _service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);

        Assert.True(result);
        _mockObjectiveRepo.Verify(r => r.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId), Times.Once);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldUpdateStatus_WhenEmployeeUpdatesOwnTask()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var userId = "testUser";
        var roles = new List<string> { "employee" };
        var newStatus = Status.InProgress;

        var objective = new Objective 
        { 
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            ExecutorId = employeeId,
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid() 
        };

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveRepo.Setup(r => r.GetObjectiveByIdAndAssigneeAsync(objectiveId, employeeId)).ReturnsAsync(objective);
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockObjectiveRepo.Setup(r => r.UpdateObjectiveAsync(objective)).Returns(Task.CompletedTask);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userId, roles);

        Assert.True(result);
        Assert.Equal(newStatus, objective.Status);
        _mockObjectiveRepo.Verify(r => r.UpdateObjectiveAsync(objective), Times.Once);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldUpdateStatus_WhenManagerUpdatesTaskInHisProject()
    {
        var objectiveId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "manager";
        var roles = new List<string> { "project manager" };
        var newStatus = Status.InProgress;

        var objective = new Objective 
        { 
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            ExecutorId = Guid.NewGuid(),
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = projectId 
        };
        
        var project = new Project
        {
            Id = projectId,
            Name = "Test", 
            CustomerName = "Test", 
            ExecutorName = "Test", 
            Priority = 1,
            DirectorId = managerId
        };

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(managerId);
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
    
        _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
    
        _mockObjectiveRepo.Setup(r => r.UpdateObjectiveAsync(objective)).Returns(Task.CompletedTask);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userId, roles);

        Assert.True(result);
        Assert.Equal(newStatus, objective.Status);
        _mockObjectiveRepo.Verify(r => r.UpdateObjectiveAsync(objective), Times.Once);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldUpdateStatus_WhenDirectorUpdatesAnyTask()
    {
        var objectiveId = Guid.NewGuid();
        var userName = "director";
        var roles = new List<string> { "director" };
        var newStatus = Status.InProgress;

        var objective = new Objective 
        { 
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            ExecutorId = Guid.NewGuid(),
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid() 
        };

        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockObjectiveRepo.Setup(r => r.UpdateObjectiveAsync(objective)).Returns(Task.CompletedTask);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userName, roles);

        Assert.True(result);
        Assert.Equal(newStatus, objective.Status);
        _mockObjectiveRepo.Verify(r => r.UpdateObjectiveAsync(objective), Times.Once);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenEmployeeUpdatesNotOwnTask()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var userId = "testUser";
        var roles = new List<string> { "employee" };
        var newStatus = Status.InProgress;

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveRepo.Setup(r => r.GetObjectiveByIdAndAssigneeAsync(objectiveId, employeeId)).ReturnsAsync((Objective?)null);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userId, roles);

        Assert.False(result);
        _mockObjectiveRepo.Verify(r => r.UpdateObjectiveAsync(It.IsAny<Objective>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenManagerUpdatesTaskOutsideHisProjects()
    {
        var objectiveId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "manager";
        var roles = new List<string> { "project manager" };
        var newStatus = Status.InProgress;

        var objective = new Objective 
        { 
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            ExecutorId = Guid.NewGuid(),
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = projectId 
        };

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(managerId);
        _mockObjectiveRepo.Setup(r => r.GetByIdAsync(objectiveId)).ReturnsAsync(objective);

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userId, roles);

        Assert.False(result);
        _mockObjectiveRepo.Verify(r => r.UpdateObjectiveAsync(It.IsAny<Objective>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatusAsync_ShouldReturnFalse_WhenUserHasNoValidRole()
    {
        var objectiveId = Guid.NewGuid();
        var userId = "user";
        var roles = new List<string> { "unauthorized" };
        var newStatus = Status.InProgress;

        var result = await _service.UpdateObjectiveStatusAsync(objectiveId, newStatus, userId, roles);

        Assert.False(result);
        _mockEmployeeRepo.Verify(r => r.GetEmployeeIdByUserIdAsync(It.IsAny<string>()), Times.Never);
        _mockObjectiveRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task GetObjectivesForManagerProjectsAsync_ShouldReturnEmptyList_WhenEmployeeNotFound()
    {
        var userId = "unknown";
        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetObjectivesForManagerProjectsAsync(userId);

        Assert.Empty(result);
        _mockObjectiveRepo.Verify(r => r.GetObjectivesByDirectorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjectsAsync_ShouldReturnObjectives_WhenEmployeeExists()
    {
        var userId = "manager";
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective>
        {
            new()
            {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
            }
        };

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveRepo.Setup(r => r.GetObjectivesByDirectorIdAsync(employeeId)).ReturnsAsync(objectives);

        var result = await _service.GetObjectivesForManagerProjectsAsync(userId);

        Assert.Equal(objectives, result);
        _mockObjectiveRepo.Verify(r => r.GetObjectivesByDirectorIdAsync(employeeId), Times.Once);
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

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockEmployeeRepo.Setup(r => r.GetObjectivesByEmployeeIdAsync(employeeId)).ReturnsAsync(objectives);

        var result = await _service.GetEmployeeObjectivesAsync(userId);

        Assert.Equal(objectives, result);
    }

    [Fact]
    public async Task GetEmployeeObjectivesAsync_ShouldReturnEmptyList_WhenEmployeeNotExists()
    {
        var userId = "Test";

        _mockEmployeeRepo.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetEmployeeObjectivesAsync(userId);

        Assert.Empty(result);
    }
}