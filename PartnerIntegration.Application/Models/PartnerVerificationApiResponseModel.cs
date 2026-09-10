using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public sealed class PartnerVerificationApiResponseModel
    {
        public PartnerData Data { get; set; } = default!;

        public StatusInfo Status { get; set; } = default!;
    }

    public sealed class PartnerData
    {
        public string PartnerId { get; set; } = string.Empty;

        public bool IsValid { get; set; }
    }

    public sealed class StatusInfo
    {
        public string Code { get; set; } = string.Empty;

        public string Msg { get; set; } = string.Empty;
    }

}
