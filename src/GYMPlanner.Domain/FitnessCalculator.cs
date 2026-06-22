using GYMPlanner.Domain.Calculations;

namespace GYMPlanner.Domain;

/// <summary>
/// Entry point for the deterministic engine: turns a <see cref="ClientProfile"/>
/// into a full set of <see cref="CalculatedMetrics"/>. Pure and side-effect free.
/// </summary>
public static class FitnessCalculator
{
    public static CalculatedMetrics Calculate(ClientProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var bmi = BodyComposition.Bmi(profile.WeightKg, profile.HeightCm);

        var bmr = Energy.BasalMetabolicRate(profile.Sex, profile.WeightKg, profile.HeightCm, profile.Age);
        var tdee = Energy.TotalDailyEnergyExpenditure(bmr, profile.ActivityLevel);
        var calorieTarget = Energy.CalorieTarget(tdee, profile.Goal);

        var macros = Nutrition.Macros(calorieTarget, profile.WeightKg, profile.Goal);
        var training = Training.Recommend(profile.DaysPerWeek, profile.MinutesPerSession, profile.Experience);

        return new CalculatedMetrics(
            Bmi: bmi,
            BmiCategory: BodyComposition.BmiCategory(bmi),
            HealthyWeightMinKg: BodyComposition.HealthyWeightMinKg(profile.HeightCm),
            HealthyWeightMaxKg: BodyComposition.HealthyWeightMaxKg(profile.HeightCm),
            BmrCalories: (int)Math.Round(bmr),
            TdeeCalories: (int)Math.Round(tdee),
            CalorieTarget: calorieTarget,
            Macros: macros,
            Training: training,
            WaterLitersPerDay: Nutrition.WaterLitersPerDay(profile.WeightKg));
    }
}
