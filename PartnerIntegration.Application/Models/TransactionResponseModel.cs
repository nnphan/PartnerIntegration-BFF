using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public class TransactionResponseModel
    {
        public string PartnerId { get; init; } = string.Empty;
        public string TransactionReference { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool PartnerVerified { get; init; }
        public bool Published { get; init; }
        public string? Message { get; init; }
    }
}
