using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Mappers;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ObjectivesController(
    IObjectiveService objectiveService, 
    IEmployeeService employeeService) : ControllerBase
{
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet]
    public async Task<ActionResult<List<ObjectiveDto>>> GetAll(
        [FromQuery] string? name = null,
        [FromQuery] List<Status>? statuses = null,
        [FromQuery] List<int>? priorities = null,
        [FromQuery] Guid? authorId = null,
        [FromQuery] Guid? executorId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isSortAscending = true)
    {
        var objectives = await objectiveService.GetAllAsync(
            name,
            statuses, 
            priorities, 
            authorId, 
            executorId, 
            projectId, 
            sortBy, 
            isSortAscending);
        return Ok(objectives.Select(ObjectiveMapper.ToDto).ToList());
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObjectiveDto>> GetById(Guid id)
    {
        var objective = await objectiveService.GetByIdAsync(id);
        if (objective == null)
            return NotFound();

        return Ok(ObjectiveMapper.ToDto(objective));
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPost]
    public async Task<ActionResult<ObjectiveDto>> Create([FromBody] ObjectiveCreateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();

        var objective = ObjectiveMapper.ToEntity(dto, (Guid)employeeId);

        await objectiveService.AddAsync(objective);

        return CreatedAtAction(nameof(GetById), new { id = objective.Id }, dto);
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ObjectiveCreateDto dto)
    {
        var existing = await objectiveService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = dto.Name;
        existing.AuthorId = dto.AuthorId;
        existing.ExecutorId = dto.ExecutorId;
        existing.Status = dto.Status;
        existing.Comment = dto.Comment;
        existing.Priority = dto.Priority;
        existing.ProjectId = dto.ProjectId;

        await objectiveService.UpdateAsync(existing);
        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await objectiveService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await objectiveService.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPost("{objectiveId:guid}/executor/{employeeId:guid}")]
    public async Task<IActionResult> AssignExecutor(Guid objectiveId, Guid employeeId)
    {
        var objective = await objectiveService.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");
        
        var employee = await employeeService.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound("Employee not found");

        var isInProject = await objectiveService.IsEmployeeInObjectiveProjectAsync(objective, employeeId);
        if (!isInProject)
            return BadRequest("Employee is not assigned to the project");

        objective.ExecutorId = employeeId;
        await objectiveService.UpdateAsync(objective);

        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPut("{objectiveId:guid}/executor")]
    public async Task<IActionResult> UpdateExecutor(Guid objectiveId, Guid employeeId)
    {
        var objective = await objectiveService.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");
        
        var employee = await employeeService.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound("Employee not found");

        objective.ExecutorId = employeeId;
        await objectiveService.UpdateAsync(objective);

        return NoContent();
    }

    [Authorize(Policy = "RequireEmployeeOrAbove")]
    [HttpPatch("{objectiveId:guid}/update-status")]
    public async Task<IActionResult> UpdateObjectiveStatus(Guid objectiveId, [FromBody] Status status)
    {
        var objective = await objectiveService.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();

        var isDirector = User.IsInRole("director");

        var success = await objectiveService.UpdateObjectiveStatusAsync(
            objective, 
            status, 
            employeeId.Value, 
            isDirector
        );

        return success ? NoContent() : Forbid();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet("my-projects-objectives")]
    public async Task<IActionResult> GetObjectivesForManagerProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();

        var objectives = await objectiveService.GetObjectivesForManagerProjectsAsync(employeeId.Value);
        return Ok(objectives.Select(ObjectiveMapper.ToDto).ToList());
    }

    [Authorize(Policy = "RequireEmployeeOrAbove")]
    [HttpGet("assigned-objectives")]
    public async Task<IActionResult> GetAssignedObjectives()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();
        
        var objectives = await objectiveService.GetEmployeeObjectivesAsync(employeeId.Value);
        return Ok(objectives.Select(ObjectiveMapper.ToDto).ToList());
    }
}