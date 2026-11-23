using EmployeeApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Employees1Controller : ControllerBase
    {
        AppDbContext _db;

        public Employees1Controller(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> Get()
        {
            var employees = await _db.Employees.ToListAsync();
            return Ok(employees);
        }

        [HttpGet("groupbydept")]
        public async Task<IActionResult> GrpByDept()
        {
            var list = await _db.Employees
                .Where(e => e.IsActive)                                   // optional: active employees only
                .OrderBy(e => e.LocationDepartment.Department.Name)
                .ThenBy(e => e.FirstName)
                .Select(e => new
                {
                    Department = e.LocationDepartment.Department.Name,
                    e.FirstName,
                    e.LastName,
                    e.JobTitle
                })
                .ToListAsync();

            var employees = await _db.Employees.ToListAsync();
            return Ok(employees);
        }

    }
}
