using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public class PartnerVerifiedResponseModel
    {
        public string PartnerId { get; init; }
        public bool IsValid { get; init; } 
    }
}
