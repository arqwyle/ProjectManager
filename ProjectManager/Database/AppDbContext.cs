using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;

namespace ProjectManager.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeProject> EmployeeProjects => Set<EmployeeProject>();
    public DbSet<Objective> Objectives => Set<Objective>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeProject>()
            .HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

        modelBuilder.Entity<EmployeeProject>()
            .HasOne(ep => ep.Employee)
            .WithMany(e => e.EmployeeProjects)
            .HasForeignKey(ep => ep.EmployeeId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<EmployeeProject>()
            .HasOne(ep => ep.Project)
            .WithMany(p => p.EmployeeProjects)
            .HasForeignKey(ep => ep.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Director)
            .WithMany()
            .HasForeignKey(p => p.DirectorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Objective>()
            .HasOne(o => o.Author)
            .WithMany(e => e.AuthoredObjectives)
            .HasForeignKey(o => o.AuthorId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<Objective>()
            .HasOne(o => o.Executor)
            .WithMany(e => e.AssignedObjectives)
            .HasForeignKey(o => o.ExecutorId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        modelBuilder.Entity<Objective>()
            .HasOne(o => o.Project)
            .WithMany(p => p.Objectives)
            .HasForeignKey(o => o.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}