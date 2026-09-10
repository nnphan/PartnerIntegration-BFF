using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Domain.Constants
{
    public class CommonConstants
    {
        public const string PartnerTransactionsMessageQueue = "partner-transactions";
        public const int MaxRetryAttemptsPolicy = 3;
        public const int TimeoutSecondsPolicy = 5;
        public static readonly int sleepDuration = 3;
        public const string Success = "success";
        public const string Error = "error";
        public const string VerificationFailedMessage = "Partner verification failed.";
        public const string PublishToQueueSuccess = "Transaction accepted and published.";
        public const string PublishToQueueFail = "Transaction verified but publishing failed.";
        public const string ApiKeyHeaderName = "X-API-KEY";


    }
}
