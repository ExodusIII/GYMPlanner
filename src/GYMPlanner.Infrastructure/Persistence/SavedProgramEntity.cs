using System.ComponentModel.DataAnnotations;

namespace GYMPlanner.Infrastructure.Persistence;

/// <summary>
/// A saved program row. The profile, metrics, and AI plan are stored as JSON
/// snapshots so a saved program stays exactly as it was generated even if the
/// engine or schema evolves later.
/// </summary>
public sealed class SavedProgramEntity
{
    public Guid Id { get; set; }

    [MaxLength(450)]
    public required string UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public required string ProfileJson { get; set; }
    public required string MetricsJson { get; set; }
    public required string ProgramJson { get; set; }
}
