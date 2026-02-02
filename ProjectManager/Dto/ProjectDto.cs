using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto;

public record ProjectDto(
    [Required] Guid Id,
    [Required] string Name,
    [Required] string CustomerName,
    [Required] string ExecutorName,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    [Required] int Priority,
    [Required] Guid DirectorId,
    List<Guid> EmployeeIds,
    List<Guid> ObjectivesIds
);