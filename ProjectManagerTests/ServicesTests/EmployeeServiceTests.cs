using Moq;
using ProjectManager.Models;
using ProjectManager.Repositories.Interfaces;
using ProjectManager.Services;

namespace ProjectManagerTests.ServicesTests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepository;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepository = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmployeesFromRepository()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = Guid.NewGuid(), 
                UserId = "1", 
                FirstName = "Test", 
                LastName = "Test", 
                Mail = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(), 
                UserId = "2", 
                FirstName = "Test", 
                LastName = "Test", 
                Mail = "Test"
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployeeFromRepository()
    {
        var id = Guid.NewGuid();
        var employee = new Employee
        {
            Id = id, UserId = "1", 
            FirstName = "Test", 
            LastName = "Test", 
            Mail = "Test"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var result = await _service.GetByIdAsync(id);

        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepositoryAddAsync()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            UserId = "1", 
            FirstName = "Test", 
            LastName = "Test", 
            Mail = "Test"
        };

        await _service.AddAsync(employee);

        _mockRepository.Verify(r => r.AddAsync(employee), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(), 
            UserId = "1", 
            FirstName = "Test", 
            LastName = "Test", 
            Mail = "Test"
        };

        await _service.UpdateAsync(employee);

        _mockRepository.Verify(r => r.UpdateAsync(employee), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
    {
        var id = Guid.NewGuid();

        await _service.DeleteAsync(id);

        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeIdByUserIdAsync_ShouldReturnIdFromRepository()
    {
        var userId = "1";
        var expectedId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync(expectedId);

        var result = await _service.GetEmployeeIdByUserIdAsync(userId);

        Assert.Equal(expectedId, result);
        _mockRepository.Verify(r => r.GetEmployeeIdByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeIdByUserIdAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var userId = "nonexistent";
        _mockRepository.Setup(r => r.GetEmployeeIdByUserIdAsync(userId)).ReturnsAsync((Guid?)null);

        var result = await _service.GetEmployeeIdByUserIdAsync(userId);

        Assert.Null(result);
        _mockRepository.Verify(r => r.GetEmployeeIdByUserIdAsync(userId), Times.Once);
    }
}