using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService service) : ControllerBase
{
    private static EmployeeDto MapToDto(Employee e)
    {
        return new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Patronymic,
            e.Mail
        );
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await service.GetAllAsync();
        return Ok(employees.Select(MapToDto).ToList());
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await service.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(MapToDto(employee));
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpPost]
    public async Task<ActionResult<EmployeeCreateDto>> Create(EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Mail = dto.Mail
        };

        await service.AddAsync(employee);

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, MapToDto(employee));
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, EmployeeDto dto)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.FirstName = dto.FirstName;
        existing.LastName = dto.LastName;
        existing.Patronymic = dto.Patronymic;
        existing.Mail = dto.Mail;

        await service.UpdateAsync(existing);
        return NoContent();
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await service.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "RequireEmployeeOrAbove")]
    [HttpGet("assigned-projects")]
    public async Task<IActionResult> GetAssignedProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var projects = await service.GetEmployeeProjectsAsync(userId);
        return Ok(projects);
    }

    [Authorize(Policy = "RequireEmployeeOrAbove")]
    [HttpGet("assigned-objectives")]
    public async Task<IActionResult> GetAssignedObjectives()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var objectives = await service.GetEmployeeObjectivesAsync(userId);
        return Ok(objectives);
    }
}