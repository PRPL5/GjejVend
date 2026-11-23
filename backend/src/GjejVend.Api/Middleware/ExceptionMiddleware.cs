using GjejVend.Application.Common;
using GjejVend.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Middleware;

/// <summary>Maps application exceptions to RFC 7807 ProblemDetails responses.</summary>
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex) when (ex is NotFoundException or ForbiddenException or ValidationFailedException or DomainException)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                _ => (StatusCodes.Status400BadRequest, "Invalid request"),
            };
            await Write(ctx, status, title, ex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled exception");
            await Write(ctx, StatusCodes.Status500InternalServerError, "Server error", "Something went wrong.");
        }
    }

    private static Task Write(HttpContext ctx, int status, string title, string detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = detail });
    }
}
