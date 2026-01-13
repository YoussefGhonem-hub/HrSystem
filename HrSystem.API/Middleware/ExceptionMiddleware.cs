using HrSystem.Shared.Common;
using System.Net;
using System.Text.Json;

namespace HrSystem.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        
        var (statusCode, message, errors) = GetExceptionDetails(exception);
        context.Response.StatusCode = statusCode;

        var response = GenericResponse.FailureResult(message, errors);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private (int statusCode, string message, string[] errors) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid request",
                new[] { exception.Message }
            ),
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid argument",
                new[] { exception.Message }
            ),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized access",
                new[] { "You are not authorized to perform this action" }
            ),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "Resource not found",
                new[] { exception.Message }
            ),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid operation",
                new[] { exception.Message }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An error occurred while processing your request",
                _environment.IsDevelopment() 
                    ? new[] { exception.Message, exception.StackTrace ?? "No stack trace available" }
                    : new[] { "An unexpected error occurred. Please try again later." }
            )
        };
    }
}
