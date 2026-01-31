using System.ComponentModel.DataAnnotations;
using ProjectManager.Models.Base;

namespace ProjectManager.Models;

public class Employee : BaseModel
{
    [MaxLength(256)]
    public required string FirstName { get; set; }
    
    [MaxLength(256)]
    public required string LastName { get; set; }
    
    [MaxLength(256)]
    public required string Patronymic { get; set; }
    
    [MaxLength(256)]
    public required string Mail { get; set; }
    
    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
    
    public ICollection<Objective> AuthoredObjectives { get; set; } = new List<Objective>();

    public ICollection<Objective> AssignedObjectives { get; set; } = new List<Objective>();
}