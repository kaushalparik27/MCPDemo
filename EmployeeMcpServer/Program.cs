using EmployeeMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Part 1: Services.
// Add the MCP server and tell it to talk to clients over HTTP.
// WithToolsFromAssembly() scans this project for classes with [McpServerToolType]
// and adds every method with [McpServerTool] to the tool list.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// A ready-to-use HttpClient for EmployeeApi.
// The base address comes from appsettings.json (EmployeeApi:BaseUrl).
builder.Services.AddHttpClient<EmployeeApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EmployeeApi:BaseUrl"]!);
});

var app = builder.Build();

// Part 2: Endpoint.
// MCP clients will connect to this URL: http://localhost:5072/mcp
app.MapMcp("/mcp");

app.Run();
