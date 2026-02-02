using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepo;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepository()
    {
        var employees = new List<Employee> { new()
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

        var result = await _service.GetAllAsync();

        Assert.Equal(employees, result);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

        var result = await _service.GetByIdAsync(id);

        Assert.Equal(employee, result);
        _mockRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        await _service.AddAsync(employee);
        _mockRepo.Verify(r => r.AddAsync(employee), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            FirstName = "Test", 
            LastName = "Test", 
            Patronymic =  "Test", 
            Mail = "Test"
        };
        await _service.UpdateAsync(employee);
        _mockRepo.Verify(r => r.UpdateAsync(employee), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        await _service.DeleteAsync(id);
        _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}