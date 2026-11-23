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

    /// <summary>
    /// Create a new employee. Returns 201 with the saved employee.
    /// Returns 400 if a value is not valid (for example the email is used already).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeSaveRequest request)
    {
        var employee = await _employees.CreateAsync(request);

        // 201 Created. The Location header points to GET /api/employees/{id}.
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>
    /// Change an employee. Send all fields. Returns 404 if the id does not exist.
    /// </summary>
    /// <param name="id">The employee id.</param>
    /// <param name="request">The new values.</param>
    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeDto>> Update(int id, EmployeeSaveRequest request)
    {
        var employee = await _employees.UpdateAsync(id, request);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    /// <summary>
    /// Delete an employee (soft delete: the row stays, but the employee is hidden).
    /// Returns 204 when done, 404 if the id does not exist,
    /// 400 if the employee is the manager of other active employees.
    /// </summary>
    /// <param name="id">The employee id.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employees.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
