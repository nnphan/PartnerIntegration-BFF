using PartnerIntegration.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Interfaces
{
    /// <summary>
    /// Defines a contract for verifying partner IDs with an external partner verification service.
    /// </summary>
    public interface IPartnerVerificationClient
    {
        Task<PartnerVerificationResultModel> VerifyPartnerAsync(string partnerId,CancellationToken cancellationToken);
    }
}
