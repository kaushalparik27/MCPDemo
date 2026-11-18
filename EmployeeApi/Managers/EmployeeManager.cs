using EmployeeApi.Data;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Managers;

// Does the database work for employees, using EF Core and LINQ.
public class EmployeeManager : IEmployee
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeManager> _logger;

    public EmployeeManager(AppDbContext db, ILogger<EmployeeManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<EmployeeDto>> SearchAsync(string? name, string? department, string? city)
    {
        _logger.LogInformation("Search employees. name={Name}, department={Department}, city={City}",
            name, department, city);

        // Start with all active employees.
        var query = _db.Employees.Where(e => e.IsActive);

        // Add a filter only when the caller gave a value.
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => (e.FirstName + " " + e.LastName).Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e => e.LocationDepartment.Department.Name == department);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(e => e.LocationDepartment.Location.City == city);
        }

        // Run the query and return flat DTO objects.
        return await ToDto(query)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Get employee by id {Id}", id);

        return await ToDto(_db.Employees.Where(e => e.EmployeeId == id))
            .FirstOrDefaultAsync();
    }

    public async Task<List<EmployeeDto>> GetByManagerNameAsync(string managerName)
    {
        _logger.LogInformation("Get employees by manager name {ManagerName}", managerName);

        // Keep active employees whose manager's full name contains the text.
        var query = _db.Employees.Where(e => e.IsActive
            && e.Manager != null
            && (e.Manager.FirstName + " " + e.Manager.LastName).Contains(managerName));

        return await ToDto(query)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    // Changes Employee rows into EmployeeDto objects.
    // EF Core adds the joins to other tables for us.
    private static IQueryable<EmployeeDto> ToDto(IQueryable<Employee> query)
    {
        return query.Select(e => new EmployeeDto
        {
            Id = e.EmployeeId,
            Name = e.FirstName + " " + e.LastName,
            Email = e.Email,
            JobTitle = e.JobTitle,
            Department = e.LocationDepartment.Department.Name,
            City = e.LocationDepartment.Location.City,
            State = e.LocationDepartment.Location.State,
            ManagerName = e.Manager == null ? null : e.Manager.FirstName + " " + e.Manager.LastName
        });
    }
}
