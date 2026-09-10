using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Application.Common;
using PartnerIntegration.Application.Models;
namespace PartnerIntegration.Api.Controllers;

[ApiController]
[Route("mock-api/partners")]
public class MockPartnerController : ControllerBase
{
    private const double FailureRate = 0.30; // 30% timeout
    private static readonly Random Random = Random.Shared;
    [HttpGet("{partnerId}")]
    public async Task<IActionResult> VerifyPartner(
        string partnerId,
        CancellationToken cancellationToken)
    {
        // 0.0 <= random < 1.0
        var random = Random.NextDouble();

        // 30% timeout
        if (random < FailureRate)
        {
            throw new TimeoutException($"Simulated timeout verifying partner '{partnerId}'.");
        }

        var result = new PartnerVerifiedResponseModel
        {
            PartnerId = partnerId,
            IsValid = true,
        };

        return Ok(ApiResponseHelper.FormatSuccess(result));
    }
}