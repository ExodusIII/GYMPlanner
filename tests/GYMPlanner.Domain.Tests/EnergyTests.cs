using GYMPlanner.Domain.Calculations;

namespace GYMPlanner.Domain.Tests;

public class EnergyTests
{
    // Mifflin-St Jeor base for weight 80kg, height 180cm, age 30:
    // 10*80 + 6.25*180 - 5*30 = 800 + 1125 - 150 = 1775
    [Theory]
    [InlineData(Sex.Male, 1780.0)]   // 1775 + 5
    [InlineData(Sex.Female, 1614.0)] // 1775 - 161
    [InlineData(Sex.Other, 1697.0)]  // 1775 - 78
    public void BasalMetabolicRate_AppliesSexOffset(Sex sex, double expected)
    {
        Assert.Equal(expected, Energy.BasalMetabolicRate(sex, 80, 180, 30), 3);
    }

    [Theory]
    [InlineData(ActivityLevel.Sedentary, 1.2)]
    [InlineData(ActivityLevel.Light, 1.375)]
    [InlineData(ActivityLevel.Moderate, 1.55)]
    [InlineData(ActivityLevel.Active, 1.725)]
    [InlineData(ActivityLevel.VeryActive, 1.9)]
    public void ActivityFactor_MapsLevel(ActivityLevel level, double expected)
    {
        Assert.Equal(expected, Energy.ActivityFactor(level));
    }

    [Fact]
    public void Tdee_MultipliesBmrByActivityFactor()
    {
        // 1780 * 1.55 = 2759
        Assert.Equal(2759.0, Energy.TotalDailyEnergyExpenditure(1780, ActivityLevel.Moderate), 3);
    }

    [Theory]
    [InlineData(Goal.LoseFat, -500)]
    [InlineData(Goal.BuildMuscle, 300)]
    [InlineData(Goal.Maintain, 0)]
    [InlineData(Goal.Recomp, -100)]
    [InlineData(Goal.Strength, 200)]
    [InlineData(Goal.Endurance, 0)]
    public void CalorieDelta_MapsGoal(Goal goal, int expected)
    {
        Assert.Equal(expected, Energy.CalorieDelta(goal));
    }

    [Fact]
    public void CalorieTarget_AppliesGoalDelta()
    {
        // 2759 - 500 = 2259
        Assert.Equal(2259, Energy.CalorieTarget(2759, Goal.LoseFat));
    }

    [Fact]
    public void CalorieTarget_FloorsAtSafeMinimum()
    {
        // Tiny TDEE with a deficit must not drop below the 1200 kcal floor.
        Assert.Equal(1200, Energy.CalorieTarget(1000, Goal.LoseFat));
    }
}
