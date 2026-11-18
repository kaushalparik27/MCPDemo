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
}
