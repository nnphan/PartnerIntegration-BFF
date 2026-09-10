using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerIntegration.Domain.Enums;


namespace PartnerIntegration.Application.Models
{
    public class PartnerTransactionModel
    {
        public string PartnerId { get; set; }
        public string TransactionReference { get; set; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public TransactionStatus Status { get; set; }
        public string? FailureReason { get; private set; }

        private PartnerTransactionModel(
        string partnerId,
        string transactionReference,
        decimal amount,
        string currency,
        DateTimeOffset timestamp)
        {
            PartnerId = partnerId;
            TransactionReference = transactionReference;
            Amount = amount;
            Currency = currency;
            Timestamp = timestamp;
            Status = TransactionStatus.Received;
        }

        public static PartnerTransactionModel Create(
        string partnerId,
        string transactionReference,
        decimal amount,
        string currency,
        DateTimeOffset timestamp)
        {
            return new PartnerTransactionModel(partnerId, transactionReference, amount, currency, timestamp);
        }

        public void MarkPartnerVerified()
        {
            Status = TransactionStatus.PartnerVerified;
        }

        public void MarkPartnerVerificationFailed()
        {
            Status = TransactionStatus.PartnerVerificationFailed;
        }

        public void MarkPublished()
        {
            Status = TransactionStatus.Published;
        }

        public void MarkPublishFailed(string reason)
        {
            Status = TransactionStatus.PublishFailed;
            FailureReason = reason;
        }
    }
}
