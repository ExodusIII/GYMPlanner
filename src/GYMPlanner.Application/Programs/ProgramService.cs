using GYMPlanner.Domain;

namespace GYMPlanner.Application.Programs;

/// <summary>
/// Orchestrates the hybrid flow for a saved program: compute the deterministic
/// metrics, have the generator write the weekly plan grounded on those numbers,
/// then persist the result for the customer.
/// </summary>
public sealed class ProgramService(IProgramGenerator generator, IProgramRepository repository)
{
    public async Task<ProgramResult> CreateAsync(
        string userId,
        ClientProfile profile,
        CancellationToken cancellationToken = default)
    {
        var metrics = FitnessCalculator.Calculate(profile);
        var program = await generator.GenerateAsync(profile, metrics, cancellationToken);
        return await repository.SaveAsync(userId, profile, metrics, program, cancellationToken);
    }

    public Task<IReadOnlyList<ProgramResult>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => repository.ListAsync(userId, cancellationToken);
}
