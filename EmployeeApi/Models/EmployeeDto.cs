namespace EmployeeApi.Models;

// The employee data we send back to the caller.
// It is flat and easy to read: no nested objects.
public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string? ManagerName { get; set; }
}
