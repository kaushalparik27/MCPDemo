using EmployeeApi.Models;
using EmployeeApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// Search employees.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployee _employees;

    public EmployeesController(IEmployee employees)
    {
        _employees = employees;
    }

    /// <summary>
    /// Search employees. All filters are optional.
    /// Example: GET /api/employees?department=Finance&amp;city=Ahmedabad
    /// </summary>
    /// <param name="name">Part of the employee name, for example "Priya" or "Shah".</param>
    /// <param name="department">Exact department name, for example "Finance".</param>
    /// <param name="city">Exact city name, for example "Ahmedabad".</param>
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> Search(string? name, string? department, string? city)
    {
        var result = await _employees.SearchAsync(name, department, city);
        return Ok(result);
    }

    /// <summary>
    /// Get all employees who report to a manager.
    /// Example: GET /api/employees/by-manager?managerName=Priya
    /// </summary>
    /// <param name="managerName">Full or part of the manager's name.</param>
    [HttpGet("by-manager")]
    public async Task<ActionResult<List<EmployeeDto>>> GetByManager(string managerName)
    {
        if (string.IsNullOrWhiteSpace(managerName))
        {
            return BadRequest("managerName is required.");
        }

        var result = await _employees.GetByManagerNameAsync(managerName);
        return Ok(result);
    }

    /// <summary>
    /// Get one employee by id. Example: GET /api/employees/5
    /// </summary>
    /// <param name="id">The employee id.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employees.GetByIdAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }
}
