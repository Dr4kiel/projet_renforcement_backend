using System.Net;
using AgentControlService.DTOs.Common;

namespace AgentControlService.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (!context.Response.HasStarted)
            {
                switch (context.Response.StatusCode)
                {
                    case (int)HttpStatusCode.NotFound:
                        await WriteJsonError(context, ApiErrorResponse.NotFound($"Route '{context.Request.Path}' not found."));
                        break;
                    case (int)HttpStatusCode.Unauthorized:
                        await WriteJsonError(context, ApiErrorResponse.Unauthorized());
                        break;
                    case (int)HttpStatusCode.Forbidden:
                        await WriteJsonError(context, ApiErrorResponse.Forbidden());
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? ex.Message
                : "An internal server error occurred.";
            await WriteJsonError(context, ApiErrorResponse.InternalServerError(message));
        }
    }

    private static async Task WriteJsonError(HttpContext context, ApiErrorResponse error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.StatusCode;
        await context.Response.WriteAsJsonAsync(error);
    }
}
