using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class ProjectControllerTests
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IObjectiveService> _mockObjectiveService;
    private readonly ProjectsController _controller;
    private readonly ClaimsPrincipal _directorUser;
    private readonly ClaimsPrincipal _managerUser;
    private readonly ClaimsPrincipal _employeeUser;

    public ProjectControllerTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockObjectiveService = new Mock<IObjectiveService>();

        _directorUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "director")
        ], "test"));

        _managerUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Role, "project manager")
        ], "test"));

        _employeeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "3"),
            new Claim(ClaimTypes.Role, "employee")
        ], "test"));

        _controller = new ProjectsController(
            _mockProjectService.Object,
            _mockEmployeeService.Object,
            _mockObjectiveService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAuthorized()
    {
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                CustomerName = "Test",
                ExecutorName = "Test",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                Priority = 1,
                DirectorId = Guid.NewGuid()
            }
        };
        _mockProjectService.Setup(s => s.GetAllAsync(null, null, null, null, null, null, true))
            .ReturnsAsync(projects);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<ProjectDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProjectExists()
    {
        var id = Guid.NewGuid();
        var project = new Project
        {
            Id = id,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(project);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProjectDto>(okResult.Value);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAt_WhenValidAndAuthorized()
    {
        var empId1 = Guid.NewGuid();
        var empId2 = Guid.NewGuid();
        var dto = new ProjectCreateDto("Test", "Test", "Test", DateTime.Now, DateTime.Now.AddDays(1), 1, Guid.NewGuid(),
            [empId1, empId2]);
        Project? capturedProject = null;

        _mockProjectService.Setup(s => s.AddAsync(It.IsAny<Project>()))
            .Callback<Project>(p => capturedProject = p)
            .Returns(Task.CompletedTask);
        _mockProjectService.Setup(s => s.AddEmployeeToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Create(dto);

        Assert.NotNull(capturedProject);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);

        var returnedDto = Assert.IsType<ProjectDto>(createdAtResult.Value);
        Assert.Equal(capturedProject.Id, returnedDto.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };
        _controller.ModelState.AddModelError("Name", "Required");

        var dto = new ProjectCreateDto("", "Test", "Test", DateTime.Now, DateTime.Now.AddDays(1), 1, Guid.NewGuid(), []);

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockProjectService.Verify(s => s.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenProjectExists()
    {
        var id = Guid.NewGuid();
        var empIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var dto = new ProjectDto(id, "Test", "Test", "Test", DateTime.Now, DateTime.Now.AddDays(1), 1, Guid.NewGuid(), empIds, []);
        var existing = new Project
        {
            Id = id,
            Name = "Old",
            CustomerName = "Old",
            ExecutorName = "Old",
            StartTime = DateTime.Now.AddDays(-1),
            EndTime = DateTime.Now,
            Priority = 999,
            DirectorId = Guid.NewGuid()
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockProjectService.Setup(s => s.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _mockProjectService.Setup(s => s.UpdateEmployeeLinksAsync(id, It.IsAny<List<Guid>>())).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(dto.Name, existing.Name);
        Assert.Equal(dto.CustomerName, existing.CustomerName);
        Assert.Equal(dto.ExecutorName, existing.ExecutorName);
        Assert.Equal(dto.StartTime, existing.StartTime);
        Assert.Equal(dto.EndTime, existing.EndTime);
        Assert.Equal(dto.Priority, existing.Priority);
        Assert.Equal(dto.DirectorId, existing.DirectorId);

        _mockProjectService.Verify(s => s.UpdateEmployeeLinksAsync(id, empIds), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var id = Guid.NewGuid();
        var dto = new ProjectDto(id, "Test", "Test", "Test", DateTime.Now, DateTime.Now.AddDays(1), 1, Guid.NewGuid(), [], []);
        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenProjectExists()
    {
        var id = Guid.NewGuid();
        var project = new Project
        {
            Id = id,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(project);
        _mockProjectService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockProjectService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UploadDocuments_ShouldReturnBadRequest_WhenNoFiles()
    {
        var projectId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.UploadDocuments(projectId, null);

        Assert.IsType<BadRequestObjectResult>(result);
        var badRequest = (BadRequestObjectResult)result;
        Assert.Equal("No files uploaded", badRequest.Value);
    }

    [Fact]
    public async Task UploadDocuments_ShouldReturnNotFound_WhenProjectNotFound()
    {
        var projectId = Guid.NewGuid();
        var files = new List<IFormFile>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1L);
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        files.Add(fileMock.Object);

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.UploadDocuments(projectId, files);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task AddEmployeeToProject_ShouldReturnNoContent_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "Test",
            LastName = "Test",
            Patronymic = "Test",
            Mail = "Test"
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockProjectService.Setup(s => s.AddEmployeeToProjectAsync(projectId, employeeId)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddEmployeeToProject(projectId, employeeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddEmployeeToProject_ShouldReturnNotFound_WhenProjectNotFound()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddEmployeeToProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task AddEmployeeToProject_ShouldReturnNotFound_WhenEmployeeNotFound()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddEmployeeToProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Employee not found", notFound.Value);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ShouldReturnNoContent_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "Test",
            LastName = "Test",
            Patronymic = "Test",
            Mail = "Test"
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockProjectService.Setup(s => s.RemoveEmployeeFromProjectAsync(projectId, employeeId)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveEmployeeFromProject(projectId, employeeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ShouldReturnNotFound_WhenProjectNotFound()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveEmployeeFromProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ShouldReturnNotFound_WhenEmployeeNotFound()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveEmployeeFromProject(projectId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Employee not found", notFound.Value);
    }

    [Fact]
    public async Task AddObjectiveToProject_ShouldReturnNoContent_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectService.Setup(s => s.AddObjectiveToProjectAsync(projectId, objectiveId)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddObjectiveToProject(projectId, objectiveId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddObjectiveToProject_ShouldReturnNotFound_WhenProjectNotFound()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddObjectiveToProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task AddObjectiveToProject_ShouldReturnNotFound_WhenObjectiveNotFound()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AddObjectiveToProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Objective not found", notFound.Value);
    }

    [Fact]
    public async Task RemoveObjectiveFromProject_ShouldReturnNoContent_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockProjectService.Setup(s => s.RemoveObjectiveFromProjectAsync(projectId, objectiveId)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveObjectiveFromProject(projectId, objectiveId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveObjectiveFromProject_ShouldReturnNotFound_WhenProjectNotFound()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveObjectiveFromProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task RemoveObjectiveFromProject_ShouldReturnNotFound_WhenObjectiveNotFound()
    {
        var projectId = Guid.NewGuid();
        var objectiveId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test",
            CustomerName = "Test",
            ExecutorName = "Test",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(1),
            Priority = 1,
            DirectorId = Guid.NewGuid()
        };

        _mockProjectService.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.RemoveObjectiveFromProject(projectId, objectiveId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Objective not found", notFound.Value);
    }

    [Fact]
    public async Task GetMyProjects_ShouldReturnOk_WhenAuthorized()
    {
        var userId = "2";
        var employeeId = Guid.NewGuid();
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                CustomerName = "Test",
                ExecutorName = "Test",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                Priority = 1,
                DirectorId = employeeId
            }
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockProjectService.Setup(s => s.GetManagerProjectsAsync(employeeId)).ReturnsAsync(projects);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetMyProjects();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProjectDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetMyProjects_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var userId = "2";
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetMyProjects();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAssignedProjects_ShouldReturnOk_WhenAuthorized()
    {
        var userId = "3";
        var employeeId = Guid.NewGuid();
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                CustomerName = "Test",
                ExecutorName = "Test",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                Priority = 1,
                DirectorId = Guid.NewGuid()
            }
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockProjectService.Setup(s => s.GetEmployeeProjectsAsync(employeeId)).ReturnsAsync(projects);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.GetAssignedProjects();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProjectDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetAssignedProjects_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var userId = "3";
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.GetAssignedProjects();

        Assert.IsType<ForbidResult>(result);
    }
}