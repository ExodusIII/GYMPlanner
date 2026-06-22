namespace GYMPlanner.Domain;

/// <summary>
/// Deterministic, formula-derived outputs for a <see cref="ClientProfile"/>.
/// These are the "hard numbers" the AI layer is grounded on when it writes the
/// human-friendly weekly program.
/// </summary>
public sealed record CalculatedMetrics(
    double Bmi,
    string BmiCategory,
    double HealthyWeightMinKg,
    double HealthyWeightMaxKg,
    int BmrCalories,
    int TdeeCalories,
    int CalorieTarget,
    MacroBreakdown Macros,
    TrainingRecommendation Training,
    double WaterLitersPerDay);

/// <summary>Daily macronutrient targets in grams, summing (approximately) to <see cref="Calories"/>.</summary>
public sealed record MacroBreakdown(
    int ProteinGrams,
    int CarbGrams,
    int FatGrams,
    int Calories);

/// <summary>Recommended training structure derived from availability and experience.</summary>
public sealed record TrainingRecommendation(
    string Split,
    int DaysPerWeek,
    int WeeklySetsPerMuscleGroup,
    int SessionMinutes);
