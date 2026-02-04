using ProjectManager.Dto;
using ProjectManager.Models;

namespace ProjectManager.Mappers;

public static class ObjectiveMapper
{
    public static ObjectiveDto ToDto(Objective entity)
    {
        return new ObjectiveDto(
            entity.Id,
            entity.Name,
            entity.AuthorId,
            entity.ExecutorId,
            entity.Status,
            entity.Comment,
            entity.Priority,
            entity.ProjectId
        );
    }

    public static Objective ToEntity(ObjectiveCreateDto dto, Guid employeeId)
    {
        return new Objective
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            AuthorId = employeeId,
            ExecutorId = dto.ExecutorId,
            Status = dto.Status,
            Comment = dto.Comment,
            Priority = dto.Priority,
            ProjectId = dto.ProjectId
        };
    }
}