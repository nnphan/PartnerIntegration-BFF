using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public class PartnerVerificationResultModel
    {
        public bool IsVerified { get; init; }
        public bool ExhaustedRetries { get; init; }
        public string? ErrorMessage { get; init; }

        public static PartnerVerificationResultModel Verified() =>
     new() { IsVerified = true, ExhaustedRetries = false };

        public static PartnerVerificationResultModel NotVerified(string reason) =>
            new() { IsVerified = false, ExhaustedRetries = false, ErrorMessage = reason };

        public static PartnerVerificationResultModel Failed(string reason) =>
            new() { IsVerified = false, ExhaustedRetries = true, ErrorMessage = reason };
    }
}
