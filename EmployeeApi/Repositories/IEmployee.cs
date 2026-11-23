using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

// The repository interface for employees.
// It says WHAT we can do. EmployeeManager says HOW.
public interface IEmployee
{
    // Search employees. Every filter is optional.
    // If a filter is null or empty, it is ignored.
    Task<List<EmployeeDto>> SearchAsync(string? name, string? department, string? city);

    // Get one employee by id. Returns null if not found.
    Task<EmployeeDto?> GetByIdAsync(int id);

    // Get all employees who report directly to a manager.
    // The manager is found by name (full or part of the name).
    Task<List<EmployeeDto>> GetByManagerNameAsync(string managerName);

    // Add a new employee. Returns the saved employee.
    // Throws ArgumentException when a value is not valid (for example the email is used already).
    Task<EmployeeDto> CreateAsync(EmployeeSaveRequest request);

    // Change an employee. Returns null if the id does not exist.
    Task<EmployeeDto?> UpdateAsync(int id, EmployeeSaveRequest request);

    // Delete an employee. We do a "soft delete": IsActive becomes false and the row stays.
    // Returns false if the id does not exist.
    Task<bool> DeleteAsync(int id);

    // The department + city choices for forms and filters.
    Task<List<LocationDepartmentDto>> GetLocationDepartmentsAsync();
}
