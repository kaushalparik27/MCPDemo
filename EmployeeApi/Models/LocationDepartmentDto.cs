namespace EmployeeApi.Models;

// One choice for the "department + city" dropdown in a form.
public class LocationDepartmentDto
{
    public int Id { get; set; }
    public string Department { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
}
