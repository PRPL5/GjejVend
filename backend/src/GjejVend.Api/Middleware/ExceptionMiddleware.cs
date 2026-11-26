using FluentValidation;
using GjejVend.Application.Common;
using GjejVend.Domain.Common;
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
        catch (Exception ex) when (ex is Application.Common.NotFoundException or Application.Common.ForbiddenException or BusinessRuleException)
        {
            var (status, title) = ex switch
            {
                Application.Common.NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                Application.Common.ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                _ => (StatusCodes.Status400BadRequest, "Business rule violated"),
            };
            await Write(context, new ProblemDetails { Status = status, Title = title, Detail = ex.Message });
        }
        catch (DomainException ex)
        {
            var status = ex switch
            {
                Domain.Common.NotFoundException => StatusCodes.Status404NotFound,
                Domain.Common.ForbiddenException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status409Conflict,
            };
            await Write(context, new ProblemDetails { Status = status, Title = ex.Message, Detail = ex.Message, Extensions = { ["code"] = ex.Code } });
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await Write(context, new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The request is not valid.",
                Extensions = { ["code"] = "validation_failed" },
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await Write(context, new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "Server error", Detail = "Something went wrong." });
        }
    }

    private static Task Write(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status!.Value;
        return context.Response.WriteAsJsonAsync(problem, problem.GetType());
    }
}
