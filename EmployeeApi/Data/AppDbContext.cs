using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Data;

// The DbContext is our connection to the MCPDemo database.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationDepartment> LocationDepartments => Set<LocationDepartment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Our tables have singular names, so we tell EF the real names.
        modelBuilder.Entity<Employee>().ToTable("Employee");
        modelBuilder.Entity<Department>().ToTable("Department");
        modelBuilder.Entity<Location>().ToTable("Location");
        modelBuilder.Entity<LocationDepartment>().ToTable("LocationDepartment");
    }
}
