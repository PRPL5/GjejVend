using GjejVend.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Middleware;

/// <summary>Turns application exceptions into RFC 7807 problem responses. Controllers stay free of try/catch.</summary>
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is NotFoundException or ForbiddenException or BusinessRuleException)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                _ => (StatusCodes.Status400BadRequest, "Business rule violated"),
            };
            await Write(context, status, title, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await Write(context, StatusCodes.Status500InternalServerError, "Server error", "Something went wrong.");
        }
    }

    private static Task Write(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = detail });
    }
}
