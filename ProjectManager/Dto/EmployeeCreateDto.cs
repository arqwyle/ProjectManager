using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto;

public record EmployeeCreateDto(
    [Required] string FirstName,
    [Required] string LastName,
    string? Patronymic,
    [Required] string Mail
);