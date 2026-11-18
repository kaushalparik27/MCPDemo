using System.ComponentModel;
using EmployeeMcpServer.Models;
using EmployeeMcpServer.Services;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace EmployeeMcpServer.Tools;

// This class holds our MCP tools.
// [McpServerToolType] tells the MCP library: "look inside this class for tools".
[McpServerToolType]
public class EmployeeTools
{
    private readonly EmployeeApiClient _api;

    // .NET gives us the EmployeeApiClient (dependency injection),
    // the same way it gave the HttpClient to EmployeeApiClient.
    public EmployeeTools(EmployeeApiClient api)
    {
        _api = api;
    }

    // [McpServerTool] tells the MCP library: "this method is a tool Claude can call".
    // [Description] is the text the LLM reads to decide WHEN to use the tool.
    [McpServerTool]
    [Description("Search employees. Use this when the user asks who works somewhere, " +
                 "or asks about an employee by name, department or city. " +
                 "All filters are optional and can be combined. " +
                 "Returns a list with name, email, job title, department, city, state and manager name.")]
    public async Task<List<EmployeeDto>> SearchEmployees(

        // [Description] on each parameter tells the LLM WHAT value to send.
        [Description("Part of the employee name, for example 'Priya' or 'Shah'. Leave empty to ignore the name.")]
        string name = "",

        [Description("Exact department name. Known departments: Finance, HR, IT, Sales. Leave empty to ignore the department.")]
        string department = "",

        [Description("Exact city name, for example 'Ahmedabad', 'Mumbai', 'Bengaluru' or 'Pune'. Leave empty to ignore the city.")]
        string city = "")
    {
        // We use "" (empty text) as the default, not null. This keeps the schema simple
        // (type "string" only). EmployeeApiClient ignores empty filters.
        // The tool only calls the API client and returns the result.
        return await _api.SearchAsync(name, department, city);
    }

    // Tool 2: get one employee by id.
    // The tool name becomes "get_employee_by_id".
    [McpServerTool]
    [Description("Get the details of one employee when you know the employee id. " +
                 "Ids are shown in the results of other employee tools. " +
                 "Returns name, email, job title, department, city, state and manager name.")]
    public async Task<EmployeeDto> GetEmployeeById(
        [Description("The employee id. A whole number, for example 8.")]
        int id)
    {
        var employee = await _api.GetByIdAsync(id);

        if (employee == null)
        {
            // McpException sends this message back to the LLM as a tool error.
            throw new McpException($"No employee was found with id {id}.");
        }

        return employee;
    }

    // Tool 3: find the people who work under a manager.
    // The tool name becomes "get_employees_by_manager".
    [McpServerTool]
    [Description("List the employees who report directly to a manager. " +
                 "Use this when the user asks who works under a person, or who is in a person's team. " +
                 "The user gives the manager's name. " +
                 "Returns name, email, job title, department, city, state and manager name for each person.")]
    public async Task<List<EmployeeDto>> GetEmployeesByManager(
        [Description("The manager's full name or part of it, for example 'Priya Shah' or 'Priya'.")]
        string managerName)
    {
        return await _api.GetByManagerAsync(managerName);
    }
}
