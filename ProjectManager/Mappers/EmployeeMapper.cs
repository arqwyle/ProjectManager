using ProjectManager.Dto;
using ProjectManager.Models;

namespace ProjectManager.Mappers;

public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Employee entity)
    {
        return new EmployeeDto(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Patronymic,
            entity.Mail,
            entity.EmployeeProjects.Select(ep => ep.ProjectId).ToList(),
            entity.AuthoredObjectives.Select(ep => ep.Id).ToList(),
            entity.AssignedObjectives.Select(ep => ep.Id).ToList()
        );
    }

    public static Employee ToEntity(EmployeeCreateDto dto)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Mail = dto.Mail
        };
    }
}