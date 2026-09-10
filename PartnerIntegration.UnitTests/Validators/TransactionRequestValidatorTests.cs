using FluentAssertions;
using PartnerIntegration.Application.Models;
using PartnerIntegration.Application.Validators;
using Xunit;

namespace PartnerIntegration.UnitTests.Validators;

public class TransactionRequestValidatorTests
{
    private readonly TransactionRequestValidator _validator = new();

    private static TransactionRequestModel ValidRequest() => new()
    {
        PartnerId = "P-1001",
        TransactionReference = "TXN-99823",
        Amount = 250,
        Currency = "USD",
        Timestamp = DateTimeOffset.UtcNow
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var request = ValidRequest();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMissingPartnerId_ShouldFail()
    {
        var request = ValidRequest() with { PartnerId = string.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TransactionRequestModel.PartnerId));
    }

    [Fact]
    public void Validate_WithMissingTransactionReference_ShouldFail()
    {
        var request = ValidRequest() with { TransactionReference = "" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TransactionRequestModel.TransactionReference));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_ShouldFail(decimal amount)
    {
        var request = ValidRequest() with { Amount = amount };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TransactionRequestModel.Amount));
    }

    [Fact]
    public void Validate_WithInvalidCurrency_ShouldFail()
    {
        var request = ValidRequest() with { Currency = "XXX" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TransactionRequestModel.Currency));
    }

    [Fact]
    public void Validate_WithMissingTimestamp_ShouldFail()
    {
        var request = ValidRequest() with { Timestamp = null };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TransactionRequestModel.Timestamp));
    }
}
