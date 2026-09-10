using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Models
{
    public class ApiResponseModel
    {
        public object Data { get; set; }
        public StatusBussiness Status { get; set; }
    }
    public class StatusBussiness
    {
        public string Code { get; set; }
        public string Msg { get; set; }
    }
}
