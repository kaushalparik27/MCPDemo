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

        // Deleted (inactive) employees are treated as not found.
        return await ToDto(_db.Employees.Where(e => e.EmployeeId == id && e.IsActive))
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

    public async Task<EmployeeDto> CreateAsync(EmployeeSaveRequest request)
    {
        _logger.LogInformation("Create employee {Email}", request.Email);

        await CheckRequestAsync(request, null);

        var employee = new Employee { IsActive = true };
        CopyValues(request, employee);

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Read it again, so the caller gets the department, city and manager name too.
        return (await GetByIdAsync(employee.EmployeeId))!;
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, EmployeeSaveRequest request)
    {
        _logger.LogInformation("Update employee {Id}", id);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id && e.IsActive);
        if (employee == null)
        {
            return null;
        }

        await CheckRequestAsync(request, id);

        CopyValues(request, employee);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete employee {Id}", id);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id && e.IsActive);
        if (employee == null)
        {
            return false;
        }

        // Do not leave people without a manager by accident.
        var hasReports = await _db.Employees.AnyAsync(e => e.ManagerId == id && e.IsActive);
        if (hasReports)
        {
            throw new ArgumentException("This employee is a manager of active employees. Change their manager first.");
        }

        // Soft delete: keep the row, only hide it.
        employee.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<LocationDepartmentDto>> GetLocationDepartmentsAsync()
    {
        return await _db.LocationDepartments
            .Select(ld => new LocationDepartmentDto
            {
                Id = ld.LocationDepartmentId,
                Department = ld.Department.Name,
                City = ld.Location.City,
                State = ld.Location.State
            })
            .OrderBy(x => x.Department).ThenBy(x => x.City)
            .ToListAsync();
    }

    // Checks the values that the [Required] attributes cannot check.
    // currentId is the employee we are updating (null when we create a new one).
    private async Task CheckRequestAsync(EmployeeSaveRequest request, int? currentId)
    {
        var email = request.Email.Trim();
        if (await _db.Employees.AnyAsync(e => e.Email == email && e.EmployeeId != currentId))
        {
            throw new ArgumentException($"The email {email} is already used.");
        }

        if (!await _db.LocationDepartments.AnyAsync(ld => ld.LocationDepartmentId == request.LocationDepartmentId))
        {
            throw new ArgumentException("This department and city combination does not exist.");
        }

        if (request.ManagerId == null)
        {
            return;
        }

        if (request.ManagerId == currentId)
        {
            throw new ArgumentException("An employee cannot be their own manager.");
        }

        if (!await _db.Employees.AnyAsync(e => e.EmployeeId == request.ManagerId && e.IsActive))
        {
            throw new ArgumentException("The manager does not exist.");
        }

        // Walk up the chain of managers. If we meet the employee, it would be a circle.
        var next = request.ManagerId;
        while (currentId != null && next != null)
        {
            if (next == currentId)
            {
                throw new ArgumentException("This manager already reports to this employee.");
            }

            next = await _db.Employees.Where(e => e.EmployeeId == next)
                .Select(e => e.ManagerId).FirstOrDefaultAsync();
        }
    }

    // Copies the values from the request to the employee row.
    private static void CopyValues(EmployeeSaveRequest request, Employee employee)
    {
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim();
        employee.JobTitle = request.JobTitle.Trim();
        employee.LocationDepartmentId = request.LocationDepartmentId;
        employee.ManagerId = request.ManagerId;
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
            ManagerName = e.Manager == null ? null : e.Manager.FirstName + " " + e.Manager.LastName,
            FirstName = e.FirstName,
            LastName = e.LastName,
            LocationDepartmentId = e.LocationDepartmentId,
            ManagerId = e.ManagerId
        });
    }
}
