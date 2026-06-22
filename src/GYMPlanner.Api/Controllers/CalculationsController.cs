using GYMPlanner.Api.Contracts;
using GYMPlanner.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GYMPlanner.Api.Controllers;

/// <summary>
/// The deterministic V1 calculator. Anonymous-friendly: anyone can compute their
/// metrics; saving a program (with the AI plan) requires an account.
/// </summary>
[ApiController]
[Route("api/calculations")]
public sealed class CalculationsController : ControllerBase
{
    [HttpPost]
    public ActionResult<CalculatedMetrics> Calculate(ClientProfile profile)
    {
        var error = ProfileValidation.Validate(profile);
        if (error is not null)
            return BadRequest(error);

        return FitnessCalculator.Calculate(profile);
    }
}
