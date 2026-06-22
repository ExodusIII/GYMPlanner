using System.Text.Json;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace GYMPlanner.Infrastructure.Persistence;

internal sealed class ProgramRepository(AppDbContext db) : IProgramRepository
{
    public async Task<ProgramResult> SaveAsync(
        string userId,
        ClientProfile profile,
        CalculatedMetrics metrics,
        WeeklyProgram program,
        CancellationToken cancellationToken = default)
    {
        var entity = new SavedProgramEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ProfileJson = JsonSerializer.Serialize(profile, AppJson.Options),
            MetricsJson = JsonSerializer.Serialize(metrics, AppJson.Options),
            ProgramJson = JsonSerializer.Serialize(program, AppJson.Options)
        };

        db.SavedPrograms.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new ProgramResult(entity.Id, entity.CreatedAt, profile, metrics, program);
    }

    public async Task<IReadOnlyList<ProgramResult>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var entities = await db.SavedPrograms
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    private static ProgramResult Map(SavedProgramEntity e) => new(
        e.Id,
        e.CreatedAt,
        JsonSerializer.Deserialize<ClientProfile>(e.ProfileJson, AppJson.Options)!,
        JsonSerializer.Deserialize<CalculatedMetrics>(e.MetricsJson, AppJson.Options)!,
        JsonSerializer.Deserialize<WeeklyProgram>(e.ProgramJson, AppJson.Options)!);
}
