using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public sealed record TransactionRequestModel
    {
        public string PartnerId { get; init; } = string.Empty;
        public string TransactionReference { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset? Timestamp { get; init; }
    }
}
