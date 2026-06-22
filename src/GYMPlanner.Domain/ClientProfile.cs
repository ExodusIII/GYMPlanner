namespace GYMPlanner.Domain;

/// <summary>
/// The full set of customer intake metrics the calculation engine needs to
/// produce a personalized program. This is a pure data record with no
/// framework dependencies so it can be reused by the web API, tests, and a
/// future mobile client.
/// </summary>
public sealed record ClientProfile
{
    public required int Age { get; init; }
    public required Sex Sex { get; init; }
    public required double HeightCm { get; init; }
    public required double WeightKg { get; init; }

    /// <summary>Optional measured body-fat percentage (0-100).</summary>
    public double? BodyFatPercent { get; init; }

    public required Goal Goal { get; init; }
    public required ActivityLevel ActivityLevel { get; init; }
    public required ExperienceLevel Experience { get; init; }
    public required Equipment Equipment { get; init; }

    /// <summary>Number of days per week the customer can train (1-7).</summary>
    public required int DaysPerWeek { get; init; }

    /// <summary>Minutes available per training session.</summary>
    public required int MinutesPerSession { get; init; }

    /// <summary>Free-text injuries or limitations to work around.</summary>
    public IReadOnlyList<string> Injuries { get; init; } = [];
}
