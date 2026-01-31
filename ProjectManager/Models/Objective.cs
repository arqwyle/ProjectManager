using System.ComponentModel.DataAnnotations;
using ProjectManager.Models.Base;

namespace ProjectManager.Models;

public class Objective : BaseModel
{
    [MaxLength(256)]
    public required string Name { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public Employee? Author { get; set; }
    
    public Guid? ExecutorId { get; set; }
    
    public Employee? Executor { get; set; }
    
    public Status Status { get; set; }
    
    [MaxLength(256)]
    public string? Comment { get; set; }
    
    public required int Priority { get; set; }
    
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }
}