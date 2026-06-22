namespace GYMPlanner.Domain.Tests;

public class FitnessCalculatorTests
{
    private static ClientProfile SampleProfile() => new()
    {
        Age = 30,
        Sex = Sex.Male,
        HeightCm = 180,
        WeightKg = 80,
        Goal = Goal.LoseFat,
        ActivityLevel = ActivityLevel.Moderate,
        Experience = ExperienceLevel.Intermediate,
        Equipment = Equipment.Gym,
        DaysPerWeek = 4,
        MinutesPerSession = 60
    };

    [Fact]
    public void Calculate_ProducesEndToEndMetricsForSampleProfile()
    {
        var metrics = FitnessCalculator.Calculate(SampleProfile());

        Assert.Equal(24.7, metrics.Bmi);
        Assert.Equal("Normal", metrics.BmiCategory);
        Assert.Equal(1780, metrics.BmrCalories);   // Mifflin male
        Assert.Equal(2759, metrics.TdeeCalories);  // 1780 * 1.55
        Assert.Equal(2259, metrics.CalorieTarget); // 2759 - 500 (LoseFat)
        Assert.Equal(176, metrics.Macros.ProteinGrams);
        Assert.Equal("Upper/Lower Split", metrics.Training.Split);
        Assert.Equal(14, metrics.Training.WeeklySetsPerMuscleGroup);
        Assert.Equal(2.8, metrics.WaterLitersPerDay);
    }

    [Fact]
    public void Calculate_ThrowsOnNullProfile()
    {
        Assert.Throws<ArgumentNullException>(() => FitnessCalculator.Calculate(null!));
    }
}
