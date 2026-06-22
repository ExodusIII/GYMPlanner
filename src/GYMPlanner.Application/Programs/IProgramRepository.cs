using GYMPlanner.Domain;

namespace GYMPlanner.Application.Programs;

/// <summary>Persistence boundary for saved programs. Implemented in Infrastructure with EF Core.</summary>
public interface IProgramRepository
{
    Task<ProgramResult> SaveAsync(
        string userId,
        ClientProfile profile,
        CalculatedMetrics metrics,
        WeeklyProgram program,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProgramResult>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
