using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidationException = PartnerIntegration.Application.Exceptions.ValidationException;

namespace PartnerIntegration.Api.GlobalExceptionHandlerMiddleware;

/// <summary>
/// It always returns a structured, client-safe response.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, extensions) = Map(exception);

        _logger.LogError(exception,
            "Unhandled exception of type {ExceptionType} processing {Method} {Path}: {Message}",
            exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail, Dictionary<string, object?>? Extensions) Map(
        Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "ValidationError",
                validationException.Message,
                new Dictionary<string, object?> { ["errors"] = validationException.Errors }),

            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                "ValidationError",
                argumentException.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                "An unexpected error occurred while processing the request.",
                null)
        };
    }
}
