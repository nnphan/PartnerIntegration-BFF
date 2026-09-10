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
        /// Retry policy for transient errors when calling the external partner verification service.
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
        /// Timeout policy for calls to the external partner verification service. If the call takes longer than the specified timeout, it will be canceled.
        /// </summary>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(CommonConstants.TimeoutSecondsPolicy));
        }
    }
}
