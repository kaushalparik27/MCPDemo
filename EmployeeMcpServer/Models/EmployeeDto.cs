namespace EmployeeMcpServer.Models;

// The employee data that EmployeeApi sends to us.
// It has the same fields as EmployeeDto in EmployeeApi.
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
