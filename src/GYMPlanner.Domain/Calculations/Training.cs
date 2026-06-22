namespace GYMPlanner.Domain.Calculations;

/// <summary>Training split selection, weekly volume, and estimated 1-rep max.</summary>
public static class Training
{
    /// <summary>Picks a sensible split given the number of training days available.</summary>
    public static string RecommendSplit(int daysPerWeek) => daysPerWeek switch
    {
        <= 2 => "Full Body",
        3 => "Full Body",
        4 => "Upper/Lower Split",
        5 => "Push/Pull/Legs + Upper/Lower",
        _ => "Push/Pull/Legs"
    };

    /// <summary>Recommended weekly working sets per muscle group, scaled by experience.</summary>
    public static int WeeklySetsPerMuscleGroup(ExperienceLevel experience) => experience switch
    {
        ExperienceLevel.Beginner => 10,
        ExperienceLevel.Intermediate => 14,
        ExperienceLevel.Advanced => 18,
        _ => 10
    };

    public static TrainingRecommendation Recommend(int daysPerWeek, int minutesPerSession, ExperienceLevel experience)
        => new(
            Split: RecommendSplit(daysPerWeek),
            DaysPerWeek: daysPerWeek,
            WeeklySetsPerMuscleGroup: WeeklySetsPerMuscleGroup(experience),
            SessionMinutes: minutesPerSession);

    /// <summary>
    /// Estimated one-rep max using the Epley formula: weight * (1 + reps/30).
    /// A single rep is already a true 1RM, so reps == 1 returns the weight unchanged
    /// (raw Epley would otherwise overestimate it by ~3%).
    /// </summary>
    public static double OneRepMaxEpley(double weight, int reps)
    {
        if (reps < 1)
            throw new ArgumentOutOfRangeException(nameof(reps), "Reps must be at least 1.");

        if (reps == 1)
            return Math.Round(weight, 1);

        return Math.Round(weight * (1 + reps / 30.0), 1);
    }
}
