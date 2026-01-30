using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await service.GetAllAsync();
        return Ok(employees.Select(MapToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await service.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(MapToDto(employee));
    }

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await service.DeleteAsync(id);
        return NoContent();
    }
    
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
}