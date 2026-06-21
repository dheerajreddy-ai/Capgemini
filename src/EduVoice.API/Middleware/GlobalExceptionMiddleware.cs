using System.Net;
using System.Text.Json;
using EduVoice.Application.Common;

namespace EduVoice.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message} | Path: {Path} | Method: {Method}",
                ex.Message, context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, code) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access", "UNAUTHORIZED"),
            ArgumentException e => (HttpStatusCode.BadRequest, e.Message, "VALIDATION_ERROR"),
            InvalidOperationException e => (HttpStatusCode.BadRequest, e.Message, "INVALID_OPERATION"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "INTERNAL_ERROR")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(message, code);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
