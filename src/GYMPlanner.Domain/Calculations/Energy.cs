namespace GYMPlanner.Domain.Calculations;

/// <summary>Basal metabolic rate, total daily energy expenditure, and the goal-adjusted calorie target.</summary>
public static class Energy
{
    /// <summary>
    /// Basal Metabolic Rate via the Mifflin-St Jeor equation (kcal/day).
    /// Men:   10*kg + 6.25*cm - 5*age + 5
    /// Women: 10*kg + 6.25*cm - 5*age - 161
    /// <see cref="Sex.Other"/> uses the average of the two sex offsets (-78).
    /// </summary>
    public static double BasalMetabolicRate(Sex sex, double weightKg, double heightCm, int age)
    {
        var baseValue = 10.0 * weightKg + 6.25 * heightCm - 5.0 * age;
        var offset = sex switch
        {
            Sex.Male => 5.0,
            Sex.Female => -161.0,
            _ => -78.0
        };
        return baseValue + offset;
    }

    /// <summary>Activity multiplier applied to BMR to estimate TDEE.</summary>
    public static double ActivityFactor(ActivityLevel level) => level switch
    {
        ActivityLevel.Sedentary => 1.2,
        ActivityLevel.Light => 1.375,
        ActivityLevel.Moderate => 1.55,
        ActivityLevel.Active => 1.725,
        ActivityLevel.VeryActive => 1.9,
        _ => 1.2
    };

    /// <summary>Total Daily Energy Expenditure (kcal/day).</summary>
    public static double TotalDailyEnergyExpenditure(double bmr, ActivityLevel level)
        => bmr * ActivityFactor(level);

    /// <summary>Calorie delta (kcal) applied to TDEE for a given goal.</summary>
    public static int CalorieDelta(Goal goal) => goal switch
    {
        Goal.LoseFat => -500,
        Goal.BuildMuscle => +300,
        Goal.Maintain => 0,
        Goal.Recomp => -100,
        Goal.Strength => +200,
        Goal.Endurance => 0,
        _ => 0
    };

    // Conservative floor so an aggressive deficit never recommends an unsafe intake.
    private const int MinimumSafeCalories = 1200;

    /// <summary>Daily calorie target (kcal) = TDEE + goal delta, floored at a safe minimum.</summary>
    public static int CalorieTarget(double tdee, Goal goal)
    {
        var target = tdee + CalorieDelta(goal);
        return Math.Max(MinimumSafeCalories, (int)Math.Round(target));
    }
}
