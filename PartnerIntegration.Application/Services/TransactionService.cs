using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using PartnerIntegration.Application.Interfaces;
using PartnerIntegration.Application.Models;
using PartnerIntegration.Domain.Enums;
using PartnerIntegration.Domain.Constants;
using ValidationException = PartnerIntegration.Application.Exceptions.ValidationException;

namespace PartnerIntegration.Application.Services
{
    public sealed class TransactionService : ITransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly IPartnerVerificationClient _partnerVerificationClient;
        private readonly IValidator<TransactionRequestModel> _validator;
        private readonly IMessagePublisher _messagePublisher;

        public TransactionService(
       IPartnerVerificationClient partnerVerificationClient,
       ILogger<TransactionService> logger,
       IValidator<TransactionRequestModel> validator,
       IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _partnerVerificationClient = partnerVerificationClient;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }
        public async Task<TransactionResponseModel> ProcessTransactionAsync(TransactionRequestModel request, CancellationToken cancellationToken)
        {
            // STEP 1
            // Validate request
            await ValidateAsync(request, cancellationToken);

            var transaction = CreatePartnerdTransaction(request);

            // STEP 2
            // Verify PartnerId valid or invalid
            var verificationResult = await _partnerVerificationClient.VerifyPartnerAsync(request.PartnerId, cancellationToken);

            if (!verificationResult.IsVerified)
            {
                transaction.MarkPartnerVerificationFailed();
                return BuildResponse(transaction, verificationResult.ErrorMessage ?? CommonConstants.VerificationFailedMessage);
            }

            // STEP 3
            // Publish message
            transaction.MarkPartnerVerified();
            var published = await TryPublishAsync(transaction, cancellationToken);

            return BuildResponse(transaction,
            message: published ? CommonConstants.PublishToQueueSuccess : CommonConstants.PublishToQueueFail);
            
        }


        //comment
        private async Task ValidateAsync(TransactionRequestModel request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(errors);
            }
        }

        private static PartnerTransactionModel CreatePartnerdTransaction(TransactionRequestModel request)
        {
            return PartnerTransactionModel.Create(
                request.PartnerId,
                request.TransactionReference,
                request.Amount,
                request.Currency,
                request.Timestamp!.Value);
        }

        private static TransactionResponseModel BuildResponse(
        PartnerTransactionModel transaction,
        string message)
        {
            return new TransactionResponseModel
            {
                PartnerId = transaction.PartnerId,
                TransactionReference = transaction.TransactionReference,
                Status = transaction.Status.ToString(),
                PartnerVerified = transaction.Status is TransactionStatus.PartnerVerified or TransactionStatus.Published or TransactionStatus.PublishFailed,
                Published = transaction.Status == TransactionStatus.Published,
                Message = message,
            };
        }

        private async Task<bool> TryPublishAsync(
        PartnerTransactionModel transaction,
        CancellationToken cancellationToken)
        {
            var message = new PartnerTransactionMessageModel
            {
                PartnerId = transaction.PartnerId,
                TransactionReference = transaction.TransactionReference,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Timestamp = transaction.Timestamp
            };

            try
            {
                await _messagePublisher.PublishAsync(
                    CommonConstants.PartnerTransactionsMessageQueue,
                    message,
                    cancellationToken);

                transaction.MarkPublished();
                _logger.LogInformation("Published transaction {TransactionReference} to queue {Queue}",
                    transaction.TransactionReference, CommonConstants.PartnerTransactionsMessageQueue);

                return true;
            }
            catch (Exception ex)
            {
                transaction.MarkPublishFailed(ex.Message);
                _logger.LogError(ex, "Failed to publish transaction {TransactionReference} to queue {Queue}",
                    transaction.TransactionReference, CommonConstants.PartnerTransactionsMessageQueue);

                return false;
            }
        }
    }
}
