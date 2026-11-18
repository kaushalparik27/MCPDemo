namespace EmployeeApi.Models;

// One row of the Employee table.
public class Employee
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public int LocationDepartmentId { get; set; }
    public int? ManagerId { get; set; }      // null = no manager
    public bool IsActive { get; set; }

    // Links to the related rows
    public LocationDepartment LocationDepartment { get; set; } = null!;
    public Employee? Manager { get; set; }
}
