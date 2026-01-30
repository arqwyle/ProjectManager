using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto;

public record ProjectCreateDto(
    [Required] string Name,
    [Required] string CustomerName,
    [Required] string ExecutorName,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    [Required] int Priority,
    [Required] Guid DirectorId,
    [Required] List<Guid> EmployeeIds
);