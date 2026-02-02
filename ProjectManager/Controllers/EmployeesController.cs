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
        var projectsIds = e.EmployeeProjects.Select(ep => ep.ProjectId).ToList();
        var authoredObjectivesIds = e.AuthoredObjectives.Select(ep => ep.Id).ToList();
        var assignedObjectivesIds = e.AssignedObjectives.Select(ep => ep.Id).ToList();
        
        return new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Patronymic,
            e.Mail,
            projectsIds,
            authoredObjectivesIds,
            assignedObjectivesIds
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
    
    [Authorize(Roles = "director")]
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
    
    [Authorize(Roles = "director")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, EmployeeDto dto)
    {
        var employee = await service.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Patronymic = dto.Patronymic;
        employee.Mail = dto.Mail;

        await service.UpdateAsync(employee);
        
        return NoContent();
    }
    
    [Authorize(Roles = "director")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await service.DeleteAsync(id);
        return NoContent();
    }
}