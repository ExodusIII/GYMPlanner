using GYMPlanner.Domain.Calculations;

namespace GYMPlanner.Domain.Tests;

public class NutritionTests
{
    [Theory]
    [InlineData(Goal.LoseFat, 2.2)]
    [InlineData(Goal.BuildMuscle, 2.0)]
    [InlineData(Goal.Maintain, 1.8)]
    [InlineData(Goal.Recomp, 2.2)]
    [InlineData(Goal.Strength, 2.0)]
    [InlineData(Goal.Endurance, 1.6)]
    public void ProteinPerKg_MapsGoal(Goal goal, double expected)
    {
        Assert.Equal(expected, Nutrition.ProteinPerKg(goal));
    }

    [Fact]
    public void Macros_SplitsCaloriesAcrossProteinFatAndCarbs()
    {
        // calories 2259, weight 80kg, LoseFat (2.2 g/kg)
        // protein = 2.2*80 = 176g (704 kcal)
        // fat = 25% of 2259 = 564.75 kcal => 62.75g -> 63g
        // carbs = (2259 - 704 - 564.75) = 990.25 kcal => 247.56g -> 248g
        var macros = Nutrition.Macros(2259, 80, Goal.LoseFat);

        Assert.Equal(176, macros.ProteinGrams);
        Assert.Equal(63, macros.FatGrams);
        Assert.Equal(248, macros.CarbGrams);
        Assert.Equal(2259, macros.Calories);
    }

    [Fact]
    public void Macros_ClampsCarbsToZeroWhenProteinAndFatExceedTarget()
    {
        // Very low calories with high bodyweight: protein+fat overshoot, carbs must clamp to 0.
        var macros = Nutrition.Macros(1200, 120, Goal.LoseFat);
        Assert.Equal(0, macros.CarbGrams);
    }

    [Fact]
    public void WaterLitersPerDay_Uses35MlPerKg()
    {
        // 80 * 35 / 1000 = 2.8
        Assert.Equal(2.8, Nutrition.WaterLitersPerDay(80));
    }
}
