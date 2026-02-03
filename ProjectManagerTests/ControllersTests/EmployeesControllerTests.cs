using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class EmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _mockService;
    private readonly EmployeesController _controller;
    private readonly ClaimsPrincipal _managerUser;
    private readonly ClaimsPrincipal _directorUser;

    public EmployeeControllerTests()
    {
        _mockService = new Mock<IEmployeeService>();
        
        _managerUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "project manager")
        ], "test"));

        _directorUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Role, "director")
        ], "test"));

        _controller = new EmployeesController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAuthorized()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Test",
                Mail = "Test"
            }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(employees);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<EmployeeDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenEmployeeExists()
    {
        var id = Guid.NewGuid();
        var employee = new Employee
        {
            Id = id,
            FirstName = "Test",
            LastName = "Test",
            Mail = "Test"
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(employee);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _managerUser }
        };

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<EmployeeDto>(okResult.Value);
        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);
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
        var dto = new EmployeeCreateDto("Test", "Test", "", "Test");
        Employee? capturedEmployee = null;

        _mockService.Setup(s => s.AddAsync(It.IsAny<Employee>()))
            .Callback<Employee>(e => capturedEmployee = e)
            .Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Create(dto);

        Assert.NotNull(capturedEmployee);
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);

        var returnedDto = Assert.IsType<EmployeeDto>(createdAtResult.Value);
        Assert.Equal(capturedEmployee.Id, returnedDto.Id);
    }

    [Fact]
    public async Task Create_ShouldCallServiceAddAsync_WithCorrectEmployee()
    {
        var dto = new EmployeeCreateDto("Test", "Test", "", "Test");
        Employee? capturedEmployee = null;

        _mockService.Setup(s => s.AddAsync(It.IsAny<Employee>()))
            .Callback<Employee>(e => capturedEmployee = e)
            .Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        await _controller.Create(dto);

        Assert.NotNull(capturedEmployee);
        Assert.Equal("Test", capturedEmployee.FirstName);
        Assert.Equal("Test", capturedEmployee.LastName);
        Assert.Equal("Test", capturedEmployee.Mail);
        Assert.NotEqual(Guid.Empty, capturedEmployee.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var dto = new EmployeeCreateDto("", "", "", "");

        _controller.ModelState.AddModelError("FirstName", "Required");

        Assert.False(_controller.ModelState.IsValid);

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockService.Verify(s => s.AddAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenEmployeeExists()
    {
        var id = Guid.NewGuid();
        var dto = new EmployeeCreateDto("Test", "Test", "", "Test");
        var existingEmployee = new Employee
        {
            Id = id,
            FirstName = "Old",
            LastName = "Old",
            Mail = "Old"
        };

        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingEmployee);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Test", existingEmployee.FirstName);
        Assert.Equal("Test", existingEmployee.LastName);
        Assert.Equal("Test", existingEmployee.Mail);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        var id = Guid.NewGuid();
        var dto = new EmployeeCreateDto("Test", "Test", "", "Test");

        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenEmployeeExists()
    {
        var id = Guid.NewGuid();
        var existing = new Employee
        {
            Id = id,
            FirstName = "Test",
            LastName = "Test",
            Mail = "Test"
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _directorUser }
        };

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }
}