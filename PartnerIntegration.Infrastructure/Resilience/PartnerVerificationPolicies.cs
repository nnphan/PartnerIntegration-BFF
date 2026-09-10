using Microsoft.Extensions.Logging;
using PartnerIntegration.Domain.Constants;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PartnerIntegration.Infrastructure.Resilience
{
    public static class PartnerVerificationPolicies
    {
        /// <summary>
        /// Retry policy: retries transient HttpRequestException / non-success status code
        /// AND TimeoutRejectedException (thrown by the timeout policy below),
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage>GetRetryPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: CommonConstants.MaxRetryAttemptsPolicy,
                    sleepDurationProvider:
                        retryAttempt =>
                            TimeSpan.FromSeconds(CommonConstants.sleepDuration),
                    onRetry: (outcome, delay, attempt, context) =>
                    {
                        logger.LogWarning(
                            "Partner verification retry attempt {Attempt} after {Delay}s. Reason={Reason}",
                            attempt, delay.TotalSeconds,
                            outcome.Exception?.Message ?? $"HTTP {(int?)outcome.Result?.StatusCode}");
                    });
        }

        /// <summary>
        /// Per-attempt timeout — ensures a single call to the external partner API
        /// cannot hang indefinitely; each attempt is bounded independently of the
        /// overall retry budget.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(CommonConstants.TimeoutSecondsPolicy));
        }
    }
}
