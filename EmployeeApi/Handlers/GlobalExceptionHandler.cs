using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Handlers;

// Catches every unhandled exception, writes it to the log,
// and sends a clean JSON error to the caller.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Write the full error (with stack trace) to the log file.
        _logger.LogError(exception, "Unhandled error on {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        // Choose the status code and a safe message for the caller.
        int statusCode;
        string title;

        if (exception is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = exception.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = exception.Message;
        }
        else
        {
            // For other errors we do not show details to the caller.
            statusCode = StatusCodes.Status500InternalServerError;
            title = "An unexpected error happened. Please try again later.";
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        // true = we handled the error
        return true;
    }
}
