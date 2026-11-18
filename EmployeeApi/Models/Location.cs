namespace EmployeeApi.Models;

// One row of the Location table.
public class Location
{
    public int LocationId { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
}
