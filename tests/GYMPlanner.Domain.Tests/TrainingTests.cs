using GYMPlanner.Domain.Calculations;

namespace GYMPlanner.Domain.Tests;

public class TrainingTests
{
    [Theory]
    [InlineData(1, "Full Body")]
    [InlineData(2, "Full Body")]
    [InlineData(3, "Full Body")]
    [InlineData(4, "Upper/Lower Split")]
    [InlineData(5, "Push/Pull/Legs + Upper/Lower")]
    [InlineData(6, "Push/Pull/Legs")]
    [InlineData(7, "Push/Pull/Legs")]
    public void RecommendSplit_MapsDaysPerWeek(int days, string expected)
    {
        Assert.Equal(expected, Training.RecommendSplit(days));
    }

    [Theory]
    [InlineData(ExperienceLevel.Beginner, 10)]
    [InlineData(ExperienceLevel.Intermediate, 14)]
    [InlineData(ExperienceLevel.Advanced, 18)]
    public void WeeklySetsPerMuscleGroup_ScalesByExperience(ExperienceLevel experience, int expected)
    {
        Assert.Equal(expected, Training.WeeklySetsPerMuscleGroup(experience));
    }

    [Fact]
    public void Recommend_AssemblesRecommendation()
    {
        var rec = Training.Recommend(daysPerWeek: 4, minutesPerSession: 60, experience: ExperienceLevel.Intermediate);

        Assert.Equal("Upper/Lower Split", rec.Split);
        Assert.Equal(4, rec.DaysPerWeek);
        Assert.Equal(14, rec.WeeklySetsPerMuscleGroup);
        Assert.Equal(60, rec.SessionMinutes);
    }

    [Fact]
    public void OneRepMaxEpley_ComputesEstimate()
    {
        // 100 * (1 + 5/30) = 116.666... -> 116.7
        Assert.Equal(116.7, Training.OneRepMaxEpley(100, 5));
    }

    [Fact]
    public void OneRepMaxEpley_ReturnsWeightForSingleRep()
    {
        Assert.Equal(100.0, Training.OneRepMaxEpley(100, 1));
    }

    [Fact]
    public void OneRepMaxEpley_ThrowsOnInvalidReps()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Training.OneRepMaxEpley(100, 0));
    }
}
