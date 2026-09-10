using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PartnerIntegration.Application.Interfaces;
using PartnerIntegration.Application.Models;
using Polly.Timeout;

namespace PartnerIntegration.Infrastructure.ExternalServices;

/// <summary>
/// Calls GET /mock-api/partners/{partnerId} through a named HttpClient whose
/// retry/timeout pipeline is registered centrally (see DependencyInjection).
/// This class stays intentionally thin: it knows how to make the call and map
/// the result — the resilience behavior itself lives in the Polly policies.
/// </summary>
public sealed class PartnerVerificationClient : IPartnerVerificationClient
{
    public const string HttpClientName = "PartnerVerificationApi";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerVerificationClient> _logger;

    public PartnerVerificationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<PartnerVerificationClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    public async Task<PartnerVerificationResultModel> VerifyPartnerAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"mock-api/partners/{Uri.EscapeDataString(partnerId)}";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;

                _logger.LogWarning(
                    "Partner verification returned non-success status {StatusCode} for partner {PartnerId}",
                    statusCode, partnerId);

                // Comment here
                // Any 5xx/408 reaching here has already been through the retry
                // policy (which retries exactly those statuses) and is therefore
                // the *final* attempt's result — i.e. retries were exhausted.
                // A genuine 4xx (e.g. 404) is not retried and is a real "not verified".
                var isRetryableStatus = statusCode == 408 || statusCode >= 500;

                return isRetryableStatus
                    ? PartnerVerificationResultModel.Failed(
                        $"Partner verification API returned status {statusCode} after exhausting all retry attempts.")
                    : PartnerVerificationResultModel.NotVerified(
                        $"Partner verification API returned status {statusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<PartnerVerificationApiResponseModel>(
                cancellationToken: cancellationToken);

            if (payload is null || !payload.Data.IsValid)
            {
                return PartnerVerificationResultModel.NotVerified("Partner is not valid.");
            }

            return PartnerVerificationResultModel.Verified();
        }       
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error verifying partner {PartnerId}.", partnerId);

            return PartnerVerificationResultModel.Failed("Partner verification failed due to a network error.");
        }
    }
}
