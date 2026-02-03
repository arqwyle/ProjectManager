using System.ComponentModel.DataAnnotations;
using ProjectManager.Models;

namespace ProjectManager.Dto;

public record ObjectiveCreateDto(
    [Required] string Name,
    [Required] Guid AuthorId,
    Guid? ExecutorId,
    Status Status,
    string? Comment,
    [Required] int Priority,
    [Required] Guid ProjectId
);