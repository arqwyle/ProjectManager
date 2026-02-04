using ProjectManager.Dto;
using ProjectManager.Models;

namespace ProjectManager.Mappers;

public static class ProjectMapper
{
    public static ProjectDto ToDto(Project entity)
    {
        return new ProjectDto(
            entity.Id,
            entity.Name,
            entity.CustomerName,
            entity.ExecutorName,
            entity.StartTime,
            entity.EndTime,
            entity.Priority,
            entity.DirectorId,
            entity.EmployeeProjects.Select(ep => ep.EmployeeId).ToList(),
            entity.Objectives.Select(o => o.Id).ToList()
        );
    }

    public static Project ToEntity(ProjectCreateDto dto)
    {
        return new Project
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
    }
}