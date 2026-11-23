using EmployeeApi.Models;
using EmployeeApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// Small lists that a web form needs, for example the dropdown choices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly IEmployee _employees;

    public LookupsController(IEmployee employees)
    {
        _employees = employees;
    }

    /// <summary>
    /// All "department + city" choices. Use the Id as LocationDepartmentId when you create or update an employee.
    /// </summary>
    [HttpGet("location-departments")]
    public async Task<ActionResult<List<LocationDepartmentDto>>> GetLocationDepartments()
    {
        return Ok(await _employees.GetLocationDepartmentsAsync());
    }
}
