namespace EmployeeApi.Models;

// One row of the LocationDepartment table.
// It says which department exists in which location.
public class LocationDepartment
{
    public int LocationDepartmentId { get; set; }
    public int LocationId { get; set; }
    public int DepartmentId { get; set; }

    // Links to the related rows
    public Location Location { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
