using GYMPlanner.Domain;

namespace GYMPlanner.Application.Programs;

/// <summary>
/// Turns a customer profile plus the deterministic metrics into a human-friendly
/// weekly program. Implemented in Infrastructure by the Claude-backed generator,
/// keeping the AI dependency out of the Application layer.
/// </summary>
public interface IProgramGenerator
{
    Task<WeeklyProgram> GenerateAsync(
        ClientProfile profile,
        CalculatedMetrics metrics,
        CancellationToken cancellationToken = default);
}
