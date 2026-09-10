using FluentValidation;
using PartnerIntegration.Application.Models;
using PartnerIntegration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Validators
{
    public class TransactionRequestValidator : AbstractValidator<TransactionRequestModel>
    {
        public TransactionRequestValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("PartnerId is required.");

            RuleFor(x => x.TransactionReference)
                .NotEmpty()
                .WithMessage("TransactionReference is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Must(BeAValidCurrency)
                .WithMessage(x => $"Currency '{x.Currency}' is not supported.");

            RuleFor(x => x.Timestamp)
                .NotNull()
                .WithMessage("Timestamp is required.");
        }

        private static bool BeAValidCurrency(string currency)
        {
            return !string.IsNullOrWhiteSpace(currency)
                && Enum.TryParse<SupportedCurrency>(currency, ignoreCase: true, out _);
        }
    }
}
