using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManager.Controllers;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManagerTests.ControllersTests;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _mockService;
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _mockService = new Mock<IEmployeeService>();
        _controller = new EmployeesController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmployees()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                FirstName = "1", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(), 
                FirstName = "2", 
                LastName = "Test", 
                Patronymic =  "Test", 
                Mail = "Test"
            }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(employees);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<EmployeeDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
        Assert.Equal("1", returnValue[0].FirstName);
        Assert.Equal("2", returnValue[1].FirstName);
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
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(employee);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<EmployeeDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEmployeeNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenValid()
    {
        var dto = new EmployeeCreateDto("Test", "Test", "Test", "Test");
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockService.Setup(s => s.AddAsync(It.IsAny<Employee>()))
                    .Callback<Employee>(e => e.Id = employee.Id)
                    .Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<EmployeeDto>(createdAtResult.Value);
        Assert.Equal(employee.Id, returnValue.Id);
        Assert.Equal("Test", returnValue.FirstName);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenValid()
    {
        var id = Guid.NewGuid();
        var dto = new EmployeeDto(Guid.NewGuid(), "Test", "Test", "Test", "Test", [], [], []);
        var existingEmployee = new Employee
        {
            Id = id, 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingEmployee);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenEmployeeNotExists()
    {
        var id = Guid.NewGuid();
        var dto = new EmployeeDto(Guid.NewGuid(), "Test", "Test", "Test", "Test", [], [], []);
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenEmployeeExists()
    {
        var id = Guid.NewGuid();
        var existingEmployee = new Employee
        {
            Id = id, 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingEmployee);
        _mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenEmployeeNotExists()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }
}