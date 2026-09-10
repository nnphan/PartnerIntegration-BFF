using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Application.Common;
using PartnerIntegration.Application.Interfaces;
using PartnerIntegration.Application.Models;

namespace PartnerIntegration.Api.Controllers
{
    
    [ApiController]
    [Route("api/v1/partner/transactions")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Receives a partner transaction, verifies the partner (with retry/backoff),
        /// and publishes the transaction to RabbitMQ if verification succeeds.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TransactionResponseModel>> SubmitTransaction(
       [FromBody] TransactionRequestModel request,
       CancellationToken cancellationToken)
        {
            var result = await _transactionService.ProcessTransactionAsync(request, cancellationToken);

            return Ok(ApiResponseHelper.FormatSuccess(result));
        }

    }
}
