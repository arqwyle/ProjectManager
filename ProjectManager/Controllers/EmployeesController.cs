using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Mappers;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService service) : ControllerBase
{
    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await service.GetAllAsync();
        return Ok(employees.Select(EmployeeMapper.ToDto).ToList());
    }
    
    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await service.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(EmployeeMapper.ToDto(employee));
    }
    
    [Authorize(Roles = nameof(Role.Director))]
    [HttpPost]
    public async Task<ActionResult<EmployeeCreateDto>> Create(EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var employee = EmployeeMapper.ToEntity(dto);

        await service.AddAsync(employee);

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, dto);
    }
    
    [Authorize(Roles = nameof(Role.Director))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, EmployeeCreateDto dto)
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
    
    [Authorize(Roles = nameof(Role.Director))]
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