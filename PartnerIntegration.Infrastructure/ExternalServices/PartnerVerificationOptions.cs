namespace PartnerIntegration.Infrastructure.ExternalServices;

/// <summary>
/// Strongly-typed configuration bound from appsettings.json ("PartnerVerificationApi"
/// section) for the outbound HttpClient that calls the (mock) partner verification API.
/// </summary>
public sealed class PartnerVerificationOptions
{
    public const string SectionName = "PartnerVerificationApi";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; init; }
}
