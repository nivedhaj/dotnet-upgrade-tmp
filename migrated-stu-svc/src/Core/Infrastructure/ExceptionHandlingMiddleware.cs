using System.Text.Json;
using Ct.Core.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ct.Core.Infrastructure;

/// <summary>
/// Modern replacement for the legacy `ExceptionHandlerMiddleware` (spec
/// section 86). Returns the legacy `ApiResponse` error envelope, never leaks
/// stack traces, and maps known exception types to sensible status codes.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var body = ApiResponse<object>.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
