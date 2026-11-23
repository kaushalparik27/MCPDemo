using System.Reflection;
using EmployeeApi.Data;
using EmployeeApi.Handlers;
using EmployeeApi.Managers;
using EmployeeApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Logging: write log files on the server with log4net.
// Log files go to the "logs" folder (see log4net.config).
// ---------------------------------------------------------------
log4net.GlobalContext.Properties["LogDir"] = Path.Combine(builder.Environment.ContentRootPath, "logs");
builder.Logging.AddLog4Net("log4net.config");

// ---------------------------------------------------------------
// Services (dependency injection)
// ---------------------------------------------------------------
builder.Services.AddControllers();

// Return errors in a standard JSON format (RFC 7807 Problem Details).
builder.Services.AddProblemDetails();

// Our own class that handles unhandled exceptions (see GlobalExceptionHandler.cs).
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Database: EF Core with SQL Server. The connection string is in appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MCPDemo")));

// When a class asks for IEmployee, give it EmployeeManager.
builder.Services.AddScoped<IEmployee, EmployeeManager>();

// CORS: lets a web app on another address (the Angular app) call this API from the browser.
// The allowed addresses are in appsettings.json (Cors:AllowedOrigins).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// Swagger: creates the API description and the Swagger UI page.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee API",
        Version = "v1",
        Description = "REST API to search employees. Used by the Employee MCP Server."
    });

    // Show the XML comments (/// summary) of controllers in Swagger UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---------------------------------------------------------------
// Middleware pipeline (the order is important)
// ---------------------------------------------------------------

// 1. Catch unhandled errors. GlobalExceptionHandler logs them and returns a clean JSON error.
app.UseExceptionHandler();
app.UseStatusCodePages();

// 2. Swagger UI: open https://localhost:7207/swagger (Development only).
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee API v1");
    });
}

// 3. Basic web middleware.
app.UseHttpsRedirection();
app.UseCors("WebApp");
app.UseAuthorization();

// 4. Endpoints.
app.MapControllers();

// A small endpoint to check that the API is running.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
   .ExcludeFromDescription();

app.Logger.LogInformation("Employee API is starting. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
