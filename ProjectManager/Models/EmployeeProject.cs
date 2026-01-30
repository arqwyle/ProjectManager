namespace ProjectManager.Models;

public class EmployeeProject
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}