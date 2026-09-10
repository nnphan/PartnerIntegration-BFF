using PartnerIntegration.Application.Models;
using PartnerIntegration.Domain.Constants;



namespace PartnerIntegration.Application.Common
{
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Format respone success
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResponseModel FormatSuccess(object data)
        {
            return new ApiResponseModel
            {
                Data = data,
                Status = new StatusBussiness
                {
                    Code = CommonConstants.Success,
                    Msg = string.Empty
                }
            };
        }

        /// <summary>
        /// Format respone error
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResponseModel FormatError(string message)
        {
            return new ApiResponseModel
            {
                Status = new StatusBussiness
                {
                    Code = CommonConstants.Error,
                    Msg = message
                }
            };
        }
    }
}
