using PartnerIntegration.Application.Models;


namespace PartnerIntegration.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionResponseModel> ProcessTransactionAsync(
        TransactionRequestModel request,
        CancellationToken cancellationToken);
    }
}
