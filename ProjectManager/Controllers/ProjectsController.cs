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
public class ProjectsController(
    IProjectService projectService, 
    IEmployeeService employeeService, 
    IObjectiveService objectiveService) : ControllerBase
{
    [Authorize(Roles = nameof(Role.Director))]
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(
        [FromQuery] string? name = null,
        [FromQuery] string? customerName = null,
        [FromQuery] string? executorName = null,
        [FromQuery] DateTime? startTimeFrom = null,
        [FromQuery] DateTime? startTimeTo = null,
        [FromQuery] List<int>? priorities = null,
        [FromQuery] Guid? directorId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isSortAscending = true)
    {
        var projects = await projectService.GetAllAsync(
            name,
            customerName, 
            executorName, 
            startTimeFrom, 
            startTimeTo, 
            priorities, 
            directorId, 
            sortBy, 
            isSortAscending);
        return Ok(projects.Select(ProjectMapper.ToDto).ToList());
    }
    
    [Authorize(Roles = nameof(Role.Director))]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);
        return project == null ? NotFound() : Ok(ProjectMapper.ToDto(project));
    }
    
    [Authorize(Roles = nameof(Role.Director))]
    [HttpPost]
    public async Task<ActionResult<ProjectCreateDto>> Create(ProjectCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var project = await projectService.CreateProjectWithEmployeesAsync(dto);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, dto);
    }

    [Authorize(Roles = nameof(Role.Director))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProjectCreateDto dto)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        project.Name = dto.Name;
        project.CustomerName = dto.CustomerName;
        project.ExecutorName = dto.ExecutorName;
        project.StartTime = dto.StartTime;
        project.EndTime = dto.EndTime;
        project.Priority = dto.Priority;
        project.DirectorId = dto.DirectorId;

        await projectService.UpdateAsync(project);
        
        await projectService.UpdateEmployeeLinksAsync(project.Id, dto.EmployeeIds);

        return NoContent();
    }

    [Authorize(Roles = nameof(Role.Director))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        await projectService.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpPost("{projectId:guid}/documents")]
    public async Task<IActionResult> UploadDocuments(Guid projectId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        var project = await projectService.GetByIdAsync(projectId);
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

    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpPost("{projectId:guid}/employees/{employeeId:guid}")]
    public async Task<IActionResult> AddEmployeeToProject(Guid projectId, Guid employeeId)
    {
        var project = await projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");
        
        var employee = await employeeService.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound("Employee not found");

        await projectService.AddEmployeeToProjectAsync(projectId, employeeId);
        return NoContent();
    }

    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpDelete("{projectId:guid}/employees/{employeeId:guid}")]
    public async Task<IActionResult> RemoveEmployeeFromProject(Guid projectId, Guid employeeId)
    {
        var project = await projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");
        
        var employee = await employeeService.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound("Employee not found");

        await projectService.RemoveEmployeeFromProjectAsync(projectId, employeeId);
        return NoContent();
    }

    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpPost("{projectId:guid}/objectives/{objectiveId:guid}")]
    public async Task<IActionResult> AddObjectiveToProject(Guid projectId, Guid objectiveId)
    {
        var project = await projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");
        
        var objective = await objectiveService.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");

        await projectService.AddObjectiveToProjectAsync(projectId, objectiveId);
        return NoContent();
    }
    
    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpDelete("{projectId:guid}/objectives/{objectiveId:guid}")]
    public async Task<IActionResult> RemoveObjectiveFromProject(Guid projectId, Guid objectiveId)
    {
        var project = await projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound("Project not found");
        
        var objective = await objectiveService.GetByIdAsync(objectiveId);
        if (objective == null)
            return NotFound("Objective not found");

        await projectService.RemoveObjectiveFromProjectAsync(projectId, objectiveId);
        return NoContent();
    }
    
    [Authorize(Policy = nameof(Policy.ManagerOrAbove))]
    [HttpGet("my-projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();

        var projects = await projectService.GetManagerProjectsAsync(employeeId.Value);
        return Ok(projects.Select(ProjectMapper.ToDto).ToList());
    }
    
    [Authorize(Policy = nameof(Policy.EmployeeOrAbove))]
    [HttpGet("assigned-projects")]
    public async Task<IActionResult> GetAssignedProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();
        
        var employeeId = await employeeService.GetEmployeeIdByUserIdAsync(userId);
        if (employeeId == null)
            return Forbid();
        
        var projects = await projectService.GetEmployeeProjectsAsync(employeeId.Value);
        return Ok(projects.Select(ProjectMapper.ToDto).ToList());
    }
}