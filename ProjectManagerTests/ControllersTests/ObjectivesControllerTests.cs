using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class ObjectiveControllerTests
{
    private readonly Mock<IObjectiveService> _mockObjectiveService;
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly ObjectivesController _controller;
    private readonly ClaimsPrincipal _managerUser;
    private readonly ClaimsPrincipal _employeeUser;

    public ObjectiveControllerTests()
    {
        _mockObjectiveService = new Mock<IObjectiveService>();
        _mockEmployeeService = new Mock<IEmployeeService>();

        _managerUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "project manager")
        ], "test"));

        _employeeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Role, "employee")
        ], "test"));

        _controller = new ObjectivesController(_mockObjectiveService.Object, _mockEmployeeService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAuthorized()
    {
        var objectives = new List<Objective>
        {
            new Objective { Id = Guid.NewGuid(), Name = "Test", Priority = 1 }
        };
        _mockObjectiveService.Setup(s => s.GetAllAsync(null, null, null, true)).ReturnsAsync(objectives);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenObjectiveExists()
    {
        var id = Guid.NewGuid();
        var objective = new Objective { Id = id, Name = "Test", Priority = 1 };
        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(objective);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ObjectiveDto>(okResult.Value);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenObjectiveDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective?)null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAt_WhenValidAndAuthorized()
    {
        var dto = new ObjectiveCreateDto("Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "", 1, Guid.NewGuid());
        var userId = "1";
        var employeeId = Guid.NewGuid();

        Objective? capturedObjective = null;

        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveService.Setup(s => s.AddAsync(It.IsAny<Objective>()))
            .Callback<Objective>(o => capturedObjective = o)
            .Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Create(dto);

        Assert.NotNull(capturedObjective);
        Assert.Equal(dto.Name, capturedObjective.Name);
        Assert.Equal(employeeId, capturedObjective.AuthorId);
        Assert.Equal(dto.ExecutorId, capturedObjective.ExecutorId);
        Assert.Equal(dto.Status, capturedObjective.Status);
        Assert.Equal(dto.Comment, capturedObjective.Comment);
        Assert.Equal(dto.Priority, capturedObjective.Priority);
        Assert.Equal(dto.ProjectId, capturedObjective.ProjectId);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);

        var returnedDto = Assert.IsType<ObjectiveDto>(createdAtResult.Value);
        Assert.Equal(capturedObjective.Id, returnedDto.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var dto = new ObjectiveCreateDto("Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "", 1, Guid.NewGuid());
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync("1")).ReturnsAsync((Guid?)null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Create(dto);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };
        _controller.ModelState.AddModelError("Name", "Required");

        var dto = new ObjectiveCreateDto("Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "", 1, Guid.NewGuid());

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockObjectiveService.Verify(s => s.AddAsync(It.IsAny<Objective>()), Times.Never);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenObjectiveExists()
    {
        var id = Guid.NewGuid();
        var dto = new ObjectiveDto(id, "Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "Test", 1, Guid.NewGuid());
        var existing = new Objective { Id = id, Name = "Old", Priority = 999 };

        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockObjectiveService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(dto.Name, existing.Name);
        Assert.Equal(dto.AuthorId, existing.AuthorId);
        Assert.Equal(dto.ExecutorId, existing.ExecutorId);
        Assert.Equal(dto.Status, existing.Status);
        Assert.Equal(dto.Comment, existing.Comment);
        Assert.Equal(dto.Priority, existing.Priority);
        Assert.Equal(dto.ProjectId, existing.ProjectId);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenObjectiveDoesNotExist()
    {
        var id = Guid.NewGuid();
        var dto = new ObjectiveDto(id, "Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "Test", 1, Guid.NewGuid());
        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenObjectiveExists()
    {
        var id = Guid.NewGuid();
        var existing = new Objective { Id = id, Name = "Test", Priority = 1 };
        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockObjectiveService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockObjectiveService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenObjectiveDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockObjectiveService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnNoContent_WhenValid()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var employee = new Employee { Id = employeeId, FirstName = "Test", LastName = "Test", Patronymic = "Test", Mail = "Test" };

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockObjectiveService.Setup(s => s.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(true);
        _mockObjectiveService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(employeeId, objective.ExecutorId);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnNotFound_WhenObjectiveNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Objective not found", notFound.Value);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnNotFound_WhenEmployeeNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Employee not found", notFound.Value);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnBadRequest_WhenEmployeeNotInProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var employee = new Employee { Id = employeeId, FirstName = "Test", LastName = "Test", Patronymic = "Test", Mail = "Test" };

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockObjectiveService.Setup(s => s.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(false);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<BadRequestObjectResult>(result);
        var badRequest = (BadRequestObjectResult)result;
        Assert.Equal("Employee is not assigned to the project", badRequest.Value);
    }

    [Fact]
    public async Task UpdateExecutor_ShouldReturnNoContent_WhenValid()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var employee = new Employee { Id = employeeId, FirstName = "Test", LastName = "Test", Patronymic = "Test", Mail = "Test" };

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockObjectiveService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.UpdateExecutor(objectiveId, employeeId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(employeeId, objective.ExecutorId);
    }

    [Fact]
    public async Task UpdateExecutor_ShouldReturnNotFound_WhenObjectiveNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.UpdateExecutor(objectiveId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Objective not found", notFound.Value);
    }

    [Fact]
    public async Task UpdateExecutor_ShouldReturnNotFound_WhenEmployeeNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.UpdateExecutor(objectiveId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Employee not found", notFound.Value);
    }

    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnNoContent_WhenAllowed()
    {
        var objectiveId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var userId = "2";
        var employeeId = Guid.NewGuid();

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveService.Setup(s => s.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false))
            .ReturnsAsync(true);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.UpdateObjectiveStatus(objectiveId, Status.Done);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnForbid_WhenNotAllowed()
    {
        var objectiveId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var userId = "2";
        var employeeId = Guid.NewGuid();

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveService.Setup(s => s.UpdateObjectiveStatusAsync(objectiveId, Status.Done, employeeId, false))
            .ReturnsAsync(false);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.UpdateObjectiveStatus(objectiveId, Status.Done);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var objectiveId = Guid.NewGuid();
        var objective = new Objective { Id = objectiveId, Name = "Test", Priority = 1 };
        var userId = "2";

        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.UpdateObjectiveStatus(objectiveId, Status.Done);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnNotFound_WhenObjectiveNotFound()
    {
        var objectiveId = Guid.NewGuid();
        _mockObjectiveService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.UpdateObjectiveStatus(objectiveId, Status.Done);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFound = (NotFoundObjectResult)result;
        Assert.Equal("Objective not found", notFound.Value);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjects_ShouldReturnOk_WhenAuthorized()
    {
        var userId = "1";
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective> { new() { Id = Guid.NewGuid(), Name = "Test", Priority = 1 } };

        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveService.Setup(s => s.GetObjectivesForManagerProjectsAsync(employeeId)).ReturnsAsync(objectives);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetObjectivesForManagerProjects();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjects_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var userId = "1";
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetObjectivesForManagerProjects();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAssignedObjectives_ShouldReturnOk_WhenAuthorized()
    {
        var userId = "2";
        var employeeId = Guid.NewGuid();
        var objectives = new List<Objective> { new() { Id = Guid.NewGuid(), Name = "Test", Priority = 1 } };

        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(employeeId);
        _mockObjectiveService.Setup(s => s.GetEmployeeObjectivesAsync(employeeId)).ReturnsAsync(objectives);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.GetAssignedObjectives();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetAssignedObjectives_ShouldReturnForbid_WhenEmployeeNotFound()
    {
        var userId = "2";
        _mockEmployeeService.Setup(s => s.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _employeeUser }
        };

        var result = await _controller.GetAssignedObjectives();

        Assert.IsType<ForbidResult>(result);
    }
}