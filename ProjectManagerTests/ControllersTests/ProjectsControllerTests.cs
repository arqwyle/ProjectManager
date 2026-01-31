using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _mockService;
    private readonly ProjectsController _controller;

    public ProjectsControllerTests()
    {
        _mockService = new Mock<IProjectService>();
        _controller = new ProjectsController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProjects()
    {
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid(), Name = "1", CustomerName = "Test", ExecutorName = "Test", Priority = 1 },
            new() { Id = Guid.NewGuid(), Name = "2", CustomerName = "Test", ExecutorName = "Test", Priority = 1 }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(projects);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<ProjectDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
        Assert.Equal("1", returnValue[0].Name);
        Assert.Equal("2", returnValue[1].Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProjectExists()
    {
        var id = Guid.NewGuid();
        var project = new Project { Id = id, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(project);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ProjectDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProjectNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenValid()
    {
        var dto = new ProjectCreateDto(
            "Test Project",
            "Customer",
            "Executor",
            DateTime.Now,
            DateTime.Now.AddDays(1),
            1,
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
        _mockService.Setup(s => s.AddEmployeeToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<ProjectDto>(createdAtResult.Value);
        Assert.Equal(project.Id, returnValue.Id);
    }

    [Fact]
    public async Task Create_ShouldAddAllEmployeesFromDto()
    {
        var empId1 = Guid.NewGuid();
        var empId2 = Guid.NewGuid();
        var dto = new ProjectCreateDto(
            "Test", "", "", DateTime.Now, DateTime.Now, 1, Guid.NewGuid(),
            [empId1, empId2]
        );
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
        _mockService.Setup(s => s.AddEmployeeToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .Returns(Task.CompletedTask);

        await _controller.Create(dto);

        _mockService.Verify(s => s.AddEmployeeToProjectAsync(project.Id, empId1), Times.Once);
        _mockService.Verify(s => s.AddEmployeeToProjectAsync(project.Id, empId2), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldHandleEmptyEmployeeList()
    {
        var dto = new ProjectCreateDto(
            "Test", "", "", DateTime.Now, DateTime.Now, 1, Guid.NewGuid(),
            []
        );
        var project = new Project { Id = Guid.NewGuid(), Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);

        await _controller.Create(dto);

        _mockService.Verify(s => s.AddEmployeeToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task AddEmployeeToProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project)null!);

        var result = await _controller.AddEmployeeToProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.Equal("Project not found", notFoundResult!.Value);
    }

    [Fact]
    public async Task AddEmployeeToProject_ShouldReturnNoContent_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project { Id = projectId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockService.Setup(s => s.AddEmployeeToProjectAsync(projectId, employeeId)).Returns(Task.CompletedTask);

        var result = await _controller.AddEmployeeToProject(projectId, employeeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project)null!);

        var result = await _controller.RemoveEmployeeFromProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.Equal("Project not found", notFoundResult!.Value);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ShouldReturnNoContent_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project { Id = projectId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockService.Setup(s => s.RemoveEmployeeFromProjectAsync(projectId, employeeId)).Returns(Task.CompletedTask);

        var result = await _controller.RemoveEmployeeFromProject(projectId, employeeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenValid()
    {
        var id = Guid.NewGuid();
        var dto = new ProjectDto(id, "Updated", "", "", DateTime.Now, DateTime.Now, 1, Guid.NewGuid(), new List<Guid>());
        var existingProject = new Project { Id = id, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingProject);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _mockService.Setup(s => s.UpdateEmployeeLinksAsync(id, It.IsAny<List<Guid>>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(It.IsAny<Project>()), Times.Once);
        _mockService.Verify(s => s.UpdateEmployeeLinksAsync(id, dto.EmployeeIds), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProjectNotExists()
    {
        var id = Guid.NewGuid();
        var dto = new ProjectDto(id, "Test", "", "", DateTime.Now, DateTime.Now, 1, Guid.NewGuid(), new List<Guid>());
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenProjectExists()
    {
        var id = Guid.NewGuid();
        var existingProject = new Project { Id = id, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingProject);
        _mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenProjectNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task AddObjectiveToProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project)null!);

        var result = await _controller.AddObjectiveToProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.Equal("Project not found", notFoundResult!.Value);
    }

    [Fact]
    public async Task AddObjectiveToProject_ShouldReturnNoContent_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project { Id = projectId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockService.Setup(s => s.AddObjectiveToProjectAsync(projectId, objectiveId)).Returns(Task.CompletedTask);

        var result = await _controller.AddObjectiveToProject(projectId, objectiveId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveObjectiveFromProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project)null!);

        var result = await _controller.RemoveObjectiveFromProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.Equal("Project not found", notFoundResult!.Value);
    }

    [Fact]
    public async Task RemoveObjectiveFromProject_ShouldReturnNoContent_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project { Id = projectId, Name = "Test", CustomerName = "Test", ExecutorName = "Test", Priority = 1 };
        _mockService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockService.Setup(s => s.RemoveObjectiveFromProjectAsync(projectId, objectiveId)).Returns(Task.CompletedTask);

        var result = await _controller.RemoveObjectiveFromProject(projectId, objectiveId);

        Assert.IsType<NoContentResult>(result);
    }
}