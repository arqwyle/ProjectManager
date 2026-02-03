using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto;

public record EmployeeDto(
    [Required] Guid Id,
    [Required] string FirstName,
    [Required] string LastName,
    string? Patronymic,
    [Required] string Mail,
    List<Guid> ProjectsIds,
    List<Guid> AuthoredObjectivesIds,
    List<Guid> AssignedObjectivesIds
);