namespace GYMPlanner.Domain.Calculations;

/// <summary>Macronutrient split and daily water intake.</summary>
public static class Nutrition
{
    private const int CaloriesPerGramProtein = 4;
    private const int CaloriesPerGramCarb = 4;
    private const int CaloriesPerGramFat = 9;

    // Fat is anchored to a fixed share of calories; protein is body-weight driven; carbs fill the rest.
    private const double FatShareOfCalories = 0.25;

    /// <summary>Protein target in grams per kg of bodyweight, by goal.</summary>
    public static double ProteinPerKg(Goal goal) => goal switch
    {
        Goal.LoseFat => 2.2,
        Goal.BuildMuscle => 2.0,
        Goal.Maintain => 1.8,
        Goal.Recomp => 2.2,
        Goal.Strength => 2.0,
        Goal.Endurance => 1.6,
        _ => 1.8
    };

    /// <summary>
    /// Splits a calorie target into protein/carb/fat grams. Protein is set from
    /// bodyweight, fat from a fixed calorie share, and carbohydrates take the
    /// remainder (clamped at zero if protein+fat already exceed the target).
    /// </summary>
    public static MacroBreakdown Macros(int calorieTarget, double weightKg, Goal goal)
    {
        var proteinGrams = ProteinPerKg(goal) * weightKg;
        var proteinCalories = proteinGrams * CaloriesPerGramProtein;

        var fatCalories = calorieTarget * FatShareOfCalories;
        var fatGrams = fatCalories / CaloriesPerGramFat;

        var carbCalories = Math.Max(0, calorieTarget - proteinCalories - fatCalories);
        var carbGrams = carbCalories / CaloriesPerGramCarb;

        return new MacroBreakdown(
            ProteinGrams: (int)Math.Round(proteinGrams),
            CarbGrams: (int)Math.Round(carbGrams),
            FatGrams: (int)Math.Round(fatGrams),
            Calories: calorieTarget);
    }

    // 35 ml of water per kg of bodyweight is a common general guideline.
    private const double MillilitresPerKg = 35.0;

    /// <summary>Recommended daily water intake in litres, rounded to one decimal.</summary>
    public static double WaterLitersPerDay(double weightKg)
        => Math.Round(weightKg * MillilitresPerKg / 1000.0, 1);
}
