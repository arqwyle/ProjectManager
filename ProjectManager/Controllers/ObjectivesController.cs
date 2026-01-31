using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ObjectivesController(IObjectiveService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ObjectiveDto>>> GetAll(
        [FromQuery] List<Status>? statuses = null,
        [FromQuery] List<int>? priorities = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isSortAscending = true)
    {
        var objectives = await service.GetAllAsync(statuses, priorities, sortBy, isSortAscending);
        return Ok(objectives.Select(MapToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObjectiveDto>> GetById(Guid id)
    {
        var objective = await service.GetByIdAsync(id);
        if (objective == null)
            return NotFound();

        return Ok(MapToDto(objective));
    }

    [HttpPost]
    public async Task<ActionResult<ObjectiveDto>> Create(ObjectiveCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var objective = new Objective
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            AuthorId = dto.AuthorId,
            ExecutorId = dto.ExecutorId,
            Status = dto.Status,
            Comment = dto.Comment,
            Priority = dto.Priority,
            ProjectId = dto.ProjectId
        };

        await service.AddAsync(objective);

        return CreatedAtAction(nameof(GetById), new { id = objective.Id }, MapToDto(objective));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ObjectiveDto dto)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = dto.Name;
        existing.AuthorId = dto.AuthorId;
        existing.ExecutorId = dto.ExecutorId;
        existing.Status = dto.Status;
        existing.Comment = dto.Comment;
        existing.Priority = dto.Priority;
        existing.ProjectId = dto.ProjectId;

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
    
    [HttpPost("{objectiveId:guid}/executor/{employeeId:guid}")]
    public async Task<IActionResult> AssignExecutor(Guid objectiveId, Guid employeeId)
    {
        var objective = await service.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");

        var isInProject = await service.IsEmployeeInObjectiveProjectAsync(objectiveId, employeeId);
        if (!isInProject)
            return BadRequest("Employee is not assigned to the project");

        objective.ExecutorId = employeeId;
        await service.UpdateAsync(objective);

        return NoContent();
    }
    
    [HttpPut("{objectiveId:guid}/executor")]
    public async Task<IActionResult> UpdateExecutor(Guid objectiveId, Guid  executorId)
    {
        var objective = await service.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound();

        objective.ExecutorId = executorId;
        await service.UpdateAsync(objective);

        return NoContent();
    }

    private static ObjectiveDto MapToDto(Objective o)
    {
        return new ObjectiveDto(
            o.Id,
            o.Name,
            o.AuthorId,
            o.ExecutorId,
            o.Status,
            o.Comment,
            o.Priority,
            o.ProjectId
        );
    }
}