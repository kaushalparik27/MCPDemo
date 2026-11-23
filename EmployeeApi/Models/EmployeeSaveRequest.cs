using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Models;

// The data a caller sends to create or update an employee.
// The [Required] and similar attributes are checked by ASP.NET Core.
// If a rule fails, the API answers 400 before our code runs.
public class EmployeeSaveRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(100)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(100)]
    public string JobTitle { get; set; } = "";

    // Which department in which city. Get the ids from GET /api/lookups/location-departments.
    [Range(1, int.MaxValue)]
    public int LocationDepartmentId { get; set; }

    // The manager's employee id. Leave it empty (null) for no manager.
    public int? ManagerId { get; set; }
}
