using Microsoft.OpenApi.Models;
using PartnerIntegration.Api.ApiKeyAuthMiddleware;
using PartnerIntegration.Api.GlobalExceptionHandlerMiddleware;
using PartnerIntegration.Domain.Constants;
using PartnerIntegration.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
builder.Services.Configure<ApiKeyOptions>(options =>
{
    options.ApiKey = builder.Configuration["ApiKey"] ?? string.Empty;
});

// ---------------------------------------------------------------------------
// Application + Infrastructure 
// ---------------------------------------------------------------------------
builder.Services.AddPartnerIntegrationServices(builder.Configuration);
//builder.Services.AddPartnerIntegrationHealthChecks();

// ---------------------------------------------------------------------------
// API layer services
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Partner Integration BFF",
        Version = "v1",
        Description = "Backend-for-Frontend microservice that receives partner transactions, " +
                      "verifies the partner via a resilient HTTP call, and publishes to RabbitMQ."
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = CommonConstants.ApiKeyHeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key required for all endpoints except /mock-api and /health."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
app.UseExceptionHandler(); // Delegates to the GlobalExceptionHandler registered via AddExceptionHandler<T>() above.

// Swagger is intentionally enabled in all environments (including the Docker
// image, which defaults to ASPNETCORE_ENVIRONMENT=Production) so the API is
// explorable out-of-the-box for this assessment. In a real production system
// this would typically be restricted to Development/Staging.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Partner Integration BFF v1");
});

app.UseHttpsRedirection();

app.UseApiKeyAuthentication();
app.MapControllers();


app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
