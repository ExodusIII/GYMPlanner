using System.Security.Claims;
using GYMPlanner.Api.Contracts;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMPlanner.Api.Controllers;

/// <summary>
/// The hybrid V2 endpoint: compute metrics, have Claude write the weekly plan,
/// persist it for the signed-in customer. Requires authentication.
/// </summary>
[ApiController]
[Route("api/programs")]
[Authorize]
public sealed class ProgramsController(ProgramService programs) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProgramResult>> Create(ClientProfile profile, CancellationToken cancellationToken)
    {
        var error = ProfileValidation.Validate(profile);
        if (error is not null)
            return BadRequest(error);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        try
        {
            return await programs.CreateAsync(userId, profile, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            // Missing API key, refusal, or unparseable output — surface a clear status.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramResult>>> List(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var results = await programs.ListAsync(userId, cancellationToken);
        return Ok(results);
    }
}
