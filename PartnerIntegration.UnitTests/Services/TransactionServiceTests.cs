using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using PartnerIntegration.Application.Models;
using PartnerIntegration.Application.Interfaces;
using PartnerIntegration.Application.Services;
using Xunit;
using ValidationException = PartnerIntegration.Application.Exceptions.ValidationException;

namespace PartnerIntegration.UnitTests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IValidator<TransactionRequestModel>> _validatorMock = new();
    private readonly Mock<IPartnerVerificationClient> _verificationClientMock = new();
    private readonly Mock<IMessagePublisher> _messagePublisherMock = new();
    private readonly Mock<ILogger<TransactionService>> _loggerMock = new();

    private readonly TransactionService _transactionServiceMock;

    public TransactionServiceTests()
    {
        _transactionServiceMock = new TransactionService(
            _verificationClientMock.Object,
            _loggerMock.Object,

            _validatorMock.Object,
            _messagePublisherMock.Object
            );
       
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static TransactionRequestModel ValidRequest() => new()
    {
        PartnerId = "P-1001",
        TransactionReference = "TXN-99823",
        Amount = 250,
        Currency = "USD",
        Timestamp = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ProcessTransactionAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        var failure = new ValidationFailure(nameof(TransactionRequestModel.Amount), "Amount must be greater than zero.");
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { failure }));

        var act = async () => await _transactionServiceMock.ProcessTransactionAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _verificationClientMock.Verify(
            c => c.VerifyPartnerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WhenPartnerVerifiedAndPublishSucceeds_ShouldReturnPublishedResponse()
    {
        _verificationClientMock
            .Setup(c => c.VerifyPartnerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PartnerVerificationResultModel.Verified());

        _messagePublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _transactionServiceMock.ProcessTransactionAsync(ValidRequest(), CancellationToken.None);

        response.PartnerVerified.Should().BeTrue();
        response.Published.Should().BeTrue();
        response.Status.Should().Be("Published");
    }

    [Fact]
    public async Task ProcessTransactionAsync_WhenPartnerNotVerified_ShouldNotPublishAndReturnUnverifiedResponse()
    {
        _verificationClientMock
            .Setup(c => c.VerifyPartnerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PartnerVerificationResultModel.NotVerified("Partner is not valid."));

        var response = await _transactionServiceMock.ProcessTransactionAsync(ValidRequest(), CancellationToken.None);

        response.PartnerVerified.Should().BeFalse();
        response.Published.Should().BeFalse();
        response.Status.Should().Be("PartnerVerificationFailed");

        _messagePublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    } 
}
