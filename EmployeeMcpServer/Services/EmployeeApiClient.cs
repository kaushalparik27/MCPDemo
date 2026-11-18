using System.Net;
using EmployeeMcpServer.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace EmployeeMcpServer.Services;

// Calls EmployeeApi over HTTP.
// The MCP tool will use this class to get employees.
public class EmployeeApiClient
{
    private readonly HttpClient _http;

    public EmployeeApiClient(HttpClient http)
    {
        _http = http;
    }

    // Calls: GET /api/employees?name=...&department=...&city=...
    // Every filter is optional.
    public async Task<List<EmployeeDto>> SearchAsync(string? name, string? department, string? city)
    {
        // Add only the filters that have a value.
        var filters = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            filters["name"] = name;
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            filters["department"] = department;
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            filters["city"] = city;
        }

        // Build the URL, for example: api/employees?department=Finance&city=Ahmedabad
        var url = QueryHelpers.AddQueryString("api/employees", filters);

        // Call the API and read the JSON answer as a list of employees.
        var employees = await _http.GetFromJsonAsync<List<EmployeeDto>>(url);

        return employees ?? new List<EmployeeDto>();
    }

    // Calls: GET /api/employees/{id}
    // Returns null when the employee does not exist (the API answers 404).
    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/employees/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmployeeDto>();
    }

    // Calls: GET /api/employees/by-manager?managerName=...
    // Returns the people who report to the manager.
    public async Task<List<EmployeeDto>> GetByManagerAsync(string managerName)
    {
        var url = QueryHelpers.AddQueryString("api/employees/by-manager", "managerName", managerName);

        var employees = await _http.GetFromJsonAsync<List<EmployeeDto>>(url);

        return employees ?? new List<EmployeeDto>();
    }
}
