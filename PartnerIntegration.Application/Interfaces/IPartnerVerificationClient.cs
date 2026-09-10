using PartnerIntegration.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Interfaces
{
    public interface IPartnerVerificationClient
    {
        Task<PartnerVerificationResultModel> VerifyPartnerAsync(
       string partnerId,
       CancellationToken cancellationToken);
    }
}
