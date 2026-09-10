using Microsoft.Extensions.Options;
using PartnerIntegration.Domain.Constants;

namespace PartnerIntegration.Api.ApiKeyAuthMiddleware;

/// <summary>
/// Configuration options for API key authentication, bound from the "ApiKey"
/// configuration value.
/// </summary>
public sealed class ApiKeyOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Lightweight API key authentication middleware. Requests to protected paths
/// must present a matching X-API-KEY header, otherwise the pipeline short-circuits
/// with 401 Unauthorized. Swagger, health checks, and the mock partner API are
/// excluded so the mock endpoint remains freely callable and docs stay accessible.
/// </summary>
public sealed class ApiKeyAuthMiddleware
{
    private static readonly string[] ExcludedPathPrefixes =
    {
        "/swagger",
        "/mock-api"
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;

    public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<ApiKeyOptions> options)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (ExcludedPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var configuredKey = options.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            // Fail closed: if no API key is configured, refuse rather than allow everything through.
            _logger.LogError("API key is not configured on the server; rejecting request to {Path}.", path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Unauthorized", status = 401, detail = "API key authentication is not configured." });
            return;
        }

        if (!context.Request.Headers.TryGetValue(CommonConstants.ApiKeyHeaderName, out var providedKey)
            || !string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejected request to {Path}: missing or invalid {Header} header.",
                path, CommonConstants.ApiKeyHeaderName);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                status = 401,
                detail = $"A valid {CommonConstants.ApiKeyHeaderName} header is required."
            });
            return;
        }

        await _next(context);
    }
}

public static class ApiKeyAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthMiddleware>();
    }
}
