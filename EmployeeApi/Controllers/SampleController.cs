using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// A simple controller to test that the API works. We can delete it later.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    private readonly ILogger<SampleController> _logger;

    public SampleController(ILogger<SampleController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns a hello message. URL: GET /api/sample
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Sample GET was called.");

        return Ok(new
        {
            message = "Hello from Employee API!",
            time = DateTime.Now
        });
    }

    /// <summary>
    /// Returns a message with the id you send. URL: GET /api/sample/5
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation("Sample GET by id was called with id {Id}.", id);

        return Ok(new
        {
            message = $"You asked for id {id}",
            id = id
        });
    }
}
