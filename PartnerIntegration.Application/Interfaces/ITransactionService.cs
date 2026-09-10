using PartnerIntegration.Application.Models;


namespace PartnerIntegration.Application.Interfaces
{
    /// <summary>
    /// Defines a contract for processing partner transactions, including verification and publishing.
    /// </summary>
    public interface ITransactionService
    {
        Task<TransactionResponseModel> ProcessTransactionAsync(TransactionRequestModel request, CancellationToken cancellationToken);
    }
}
