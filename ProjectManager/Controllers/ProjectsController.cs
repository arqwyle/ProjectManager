using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService service) : ControllerBase
{
    private static ProjectDto MapToDto(Project p)
    {
        var employeeIds = p.EmployeeProjects.Select(ep => ep.EmployeeId).ToList();
        
        return new ProjectDto(
            p.Id,
            p.Name,
            p.CustomerName,
            p.ExecutorName,
            p.StartTime,
            p.EndTime,
            p.Priority,
            p.DirectorId,
            employeeIds
        );
    }

    [Authorize(Roles = "руководитель")]
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(
        [FromQuery] string? customerName = null,
        [FromQuery] string? executorName = null,
        [FromQuery] DateTime? startTimeFrom = null,
        [FromQuery] DateTime? startTimeTo = null,
        [FromQuery] List<int>? priorities = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isSortAscending = true)
    {
        var projects = await service.GetAllAsync(customerName, executorName, startTimeFrom, startTimeTo, priorities, sortBy, isSortAscending);
        return Ok(projects.Select(MapToDto).ToList());
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        var project = await service.GetByIdAsync(id);
        return project == null ? NotFound() : Ok(MapToDto(project));
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpPost]
    public async Task<ActionResult<ProjectCreateDto>> Create(ProjectCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CustomerName = dto.CustomerName,
            ExecutorName = dto.ExecutorName,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Priority = dto.Priority,
            DirectorId = dto.DirectorId
        };

        var created = await service.AddAsync(project);

        foreach (var empId in dto.EmployeeIds)
            await service.AddEmployeeToProjectAsync(created.Id, empId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPost("{projectId:guid}/documents")]
    public async Task<IActionResult> UploadDocuments(Guid projectId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        var project = await service.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");

        var uploadPath = Path.Combine("uploads", projectId.ToString());
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            if (file.Length <= 0) continue;
            var filePath = Path.Combine(uploadPath, file.FileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        return Ok("Documents uploaded successfully");
    }

    [Authorize(Roles = "руководитель")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProjectDto dto)
    {
        var project = await service.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        project.Name = dto.Name;
        project.CustomerName = dto.CustomerName;
        project.ExecutorName = dto.ExecutorName;
        project.StartTime = dto.StartTime;
        project.EndTime = dto.EndTime;
        project.Priority = dto.Priority;
        project.DirectorId = dto.DirectorId;

        await service.UpdateAsync(project);
        
        await service.UpdateEmployeeLinksAsync(project.Id, dto.EmployeeIds);

        return NoContent();
    }
    
    [Authorize(Roles = "руководитель")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var project = await service.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        await service.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPost("{projectId:guid}/employees/{employeeId:guid}")]
    public async Task<IActionResult> AddEmployeeToProject(Guid projectId, Guid employeeId)
    {
        var project = await service.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");

        await service.AddEmployeeToProjectAsync(projectId, employeeId);
        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpDelete("{projectId:guid}/employees/{employeeId:guid}")]
    public async Task<IActionResult> RemoveEmployeeFromProject(Guid projectId, Guid employeeId)
    {
        var project = await service.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");

        await service.RemoveEmployeeFromProjectAsync(projectId, employeeId);
        return NoContent();
    }

    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpPost("{projectId:guid}/objectives/{objectiveId:guid}")]
    public async Task<IActionResult> AddObjectiveToProject(Guid projectId, Guid objectiveId)
    {
        var project = await service.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");

        await service.AddObjectiveToProjectAsync(projectId, objectiveId);
        return NoContent();
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpDelete("{projectId:guid}/objectives/{objectiveId:guid}")]
    public async Task<IActionResult> RemoveObjectiveFromProject(Guid projectId, Guid objectiveId)
    {
        var project = await service.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");

        await service.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
        return NoContent();
    }
    
    [Authorize(Policy = "RequireManagerOrAbove")]
    [HttpGet("my-projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var projects = await service.GetManagerProjectsAsync(userId);
        return Ok(projects);
    }
}