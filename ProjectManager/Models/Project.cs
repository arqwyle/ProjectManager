using System.ComponentModel.DataAnnotations;
using ProjectManager.Models.Base;

namespace ProjectManager.Models;

public class Project : BaseModel
{
    [MaxLength(256)]
    public required string Name { get; set; }
    
    [MaxLength(256)]
    public required string CustomerName { get; set; }
    
    [MaxLength(256)]
    public required string ExecutorName { get; set; }
    
    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
    
    public Guid DirectorId { get; set; }
    
    public Employee? Director { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public required int Priority { get; set; }
    
    public ICollection<Objective> Objectives { get; set; } = new List<Objective>();
}