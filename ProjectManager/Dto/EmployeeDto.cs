using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto;

public record EmployeeDto(
    [Required] Guid Id,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Patronymic,
    [Required] string Mail
);