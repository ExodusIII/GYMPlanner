using GYMPlanner.Domain;

namespace GYMPlanner.Application.Programs;

/// <summary>A persisted program: the inputs, the deterministic metrics, and the AI plan.</summary>
public sealed record ProgramResult(
    Guid Id,
    DateTimeOffset CreatedAt,
    ClientProfile Profile,
    CalculatedMetrics Metrics,
    WeeklyProgram Program);
