using System.Net;
using System.Text.Json;
using server.DTOs.Common;

namespace server.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
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

            // Handle non-success status codes that weren't handled by controllers
            if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
            {
                await HandleStatusCodeAsync(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleStatusCodeAsync(HttpContext context)
    {
        // Skip if response already has content
        if (context.Response.ContentLength > 0)
            return;

        var response = context.Response.StatusCode switch
        {
            401 => ApiErrorResponse.Unauthorized(),
            403 => ApiErrorResponse.Forbidden(),
            404 => ApiErrorResponse.NotFound("The requested endpoint was not found"),
            405 => ApiErrorResponse.Create(405, "Method not allowed"),
            _ => ApiErrorResponse.Create(context.Response.StatusCode, "An error occurred")
        };

        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var message = _environment.IsDevelopment()
            ? exception.Message
            : "An error occurred while processing your request";

        var errors = _environment.IsDevelopment()
            ? new Dictionary<string, string[]> { { "exception", new[] { exception.StackTrace ?? "" } } }
            : null;

        var response = ApiErrorResponse.InternalServerError(message);
        if (errors != null)
            response.Errors = errors;

        await WriteJsonResponseAsync(context, response);
    }

    private static async Task WriteJsonResponseAsync(HttpContext context, ApiErrorResponse response)
    {
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
