using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class ObjectivesControllerTests
{
    private readonly Mock<IObjectiveService> _mockService;
    private readonly ObjectivesController _controller;

    public ObjectivesControllerTests()
    {
        _mockService = new Mock<IObjectiveService>();
        _controller = new ObjectivesController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnObjectives()
    {
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
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(objectives);

        var result = await _controller.GetAll();

        Assert.IsType<ActionResult<List<ObjectiveDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetById_ShouldReturnObjective_WhenExists()
    {
        var id = Guid.NewGuid();
        var objective = new Objective
        {
            Id = id, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Comment = "Test", 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(objective);

        var result = await _controller.GetById(id);

        Assert.IsType<ActionResult<ObjectiveDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ObjectiveDto>(okResult.Value);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective)null!);

        var result = await _controller.GetById(id);

        Assert.IsType<ActionResult<ObjectiveDto>>(result);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
public async Task Create_ShouldReturnBadRequest_WhenModelStateInvalid()
{
    var user = new ClaimsPrincipal(new ClaimsIdentity([
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    ], "test"));
    _controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext { User = user }
    };

    _controller.ModelState.AddModelError("Name", "Required");
    var dto = new ObjectiveCreateDto("Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "", 1, Guid.NewGuid());

    var result = await _controller.Create(dto);

    Assert.IsType<BadRequestObjectResult>(result.Result);
}

[Fact]
public async Task Create_ShouldReturnCreated_WhenValid()
{
    var userId = Guid.NewGuid().ToString();
    var user = new ClaimsPrincipal(new ClaimsIdentity([
        new Claim(ClaimTypes.NameIdentifier, userId)
    ], "test"));
    _controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext { User = user }
    };

    var dto = new ObjectiveCreateDto("Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "", 1, Guid.NewGuid());
    var objective = new Objective
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        AuthorId = Guid.NewGuid(),
        ExecutorId = dto.ExecutorId,
        Status = dto.Status,
        Priority = dto.Priority,
        ProjectId = dto.ProjectId
    };

    _mockService.Setup(s => s.GetEmployeeIdByUserId(userId))
        .ReturnsAsync(objective.AuthorId);
    _mockService.Setup(s => s.AddAsync(It.IsAny<Objective>()))
        .Callback<Objective>(e => e.Id = objective.Id)
        .Returns(Task.CompletedTask);

    var result = await _controller.Create(dto);

    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    var dtoResult = Assert.IsType<ObjectiveDto>(createdResult.Value);
    Assert.Equal(objective.Id, dtoResult.Id);
}

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        var dto = new ObjectiveDto(id, "Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "Test", 1, Guid.NewGuid());
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective)null!);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenExists()
    {
        var id = Guid.NewGuid();
        var dto = new ObjectiveDto(id, "Test", Guid.NewGuid(), Guid.NewGuid(), Status.ToDo, "Test", 1, Guid.NewGuid());
        var existing = new Objective
        {
            Id = id, 
            Name = "Old", 
            AuthorId = Guid.NewGuid(), 
            Priority = 0, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Objective)null!);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenExists()
    {
        var id = Guid.NewGuid();
        var existing = new Objective
        {
            Id = id, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnNotFound_WhenObjectiveNotExists()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective)null!);

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Objective not found", notFoundResult.Value);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnBadRequest_WhenEmployeeNotInProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        };
        _mockService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockService.Setup(s => s.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(false);

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Employee is not assigned to the project", badRequestResult.Value);
    }

    [Fact]
    public async Task AssignExecutor_ShouldReturnNoContent_WhenEmployeeInProject()
    {
        var objectiveId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var objective = new Objective
        {
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
        };
        _mockService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(objective);
        _mockService.Setup(s => s.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId)).ReturnsAsync(true);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        var result = await _controller.AssignExecutor(objectiveId, employeeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateExecutor_ShouldReturnNotFound_WhenNotExists()
    {
        var objectiveId = Guid.NewGuid();
        var executorId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync((Objective)null!);

        var result = await _controller.UpdateExecutor(objectiveId, executorId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateExecutor_ShouldReturnNoContent_WhenExists()
    {
        var objectiveId = Guid.NewGuid();
        var executorId = Guid.NewGuid();
        var existing = new Objective 
        { 
            Id = objectiveId, 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid() 
        };
        _mockService.Setup(s => s.GetByIdAsync(objectiveId)).ReturnsAsync(existing);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Objective>())).Returns(Task.CompletedTask);

        var result = await _controller.UpdateExecutor(objectiveId, executorId);

        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnNoContent_WhenSuccess()
    {
        var objectiveId = Guid.NewGuid();
        var status = Status.InProgress;
        var userId = "testUser";
        var roles = new List<string> { "employee" };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, roles[0])
                ]))
            }
        };

        _mockService.Setup(s => s.UpdateObjectiveStatusAsync(objectiveId, status, userId, roles))
            .ReturnsAsync(true);

        var result = await _controller.UpdateObjectiveStatus(objectiveId, status);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateObjectiveStatus_ShouldReturnForbid_WhenFailed()
    {
        var objectiveId = Guid.NewGuid();
        var status = Status.InProgress;
        var userName = "testUser";
        var roles = new List<string> { "employee" };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, roles[0])
                ]))
            }
        };

        _mockService.Setup(s => s.UpdateObjectiveStatusAsync(objectiveId, status, userName, roles))
            .ReturnsAsync(false);

        var result = await _controller.UpdateObjectiveStatus(objectiveId, status);

        Assert.IsType<ForbidResult>(result);
    }
    
    [Fact]
    public async Task GetObjectivesForManagerProjects_ShouldReturnForbid_WhenUserIdentityIsNull()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.GetObjectivesForManagerProjects();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjects_ShouldReturnForbid_WhenUserNameIsNullOrEmpty()
    {
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, ""));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claimsIdentity) }
        };

        var result = await _controller.GetObjectivesForManagerProjects();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetObjectivesForManagerProjects_ShouldReturnOkWithObjectives_WhenValidUser()
    {
        var userId = "manager";
        var objectives = new List<Objective>
        {
            new()
            {
            Id = Guid.NewGuid(), 
            Name = "Test", 
            AuthorId = Guid.NewGuid(), 
            Priority = 1, 
            Status = Status.ToDo, 
            ProjectId = Guid.NewGuid()
            }
        };

        _mockService.Setup(s => s.GetObjectivesForManagerProjectsAsync(userId))
            .ReturnsAsync(objectives);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)]))
            }
        };

        var result = await _controller.GetObjectivesForManagerProjects();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Equal(objectives[0].Id, returnValue[0].Id);
    }

    [Fact]
    public async Task GetAssignedObjectives_ShouldReturnOkWithObjectives()
    {
        var userId = "testUser";
        var objectives = new List<Objective>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                Name = "Test", 
                AuthorId = Guid.NewGuid(), 
                Priority = 1, 
                Status = Status.ToDo, 
                ProjectId = Guid.NewGuid()
            }
        };
    
        _mockService.Setup(s => s.GetEmployeeObjectivesAsync(userId)).ReturnsAsync(objectives);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)
            ])) }
        };

        var result = await _controller.GetAssignedObjectives();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ObjectiveDto>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal("Test", returnValue[0].Name);
    }
}