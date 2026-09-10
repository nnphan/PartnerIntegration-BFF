using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Domain.Enums
{
    public enum TransactionStatus
    {
        Received = 0,
        PartnerVerificationFailed = 1,
        PartnerVerified = 2,
        Published = 3,
        PublishFailed = 4
    }
}
