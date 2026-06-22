using GYMPlanner.Domain.Calculations;

namespace GYMPlanner.Domain.Tests;

public class BodyCompositionTests
{
    [Fact]
    public void Bmi_ComputesAndRoundsToOneDecimal()
    {
        // 80 / (1.80^2) = 24.691... => 24.7
        Assert.Equal(24.7, BodyComposition.Bmi(80, 180));
    }

    [Fact]
    public void Bmi_ThrowsOnNonPositiveHeight()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BodyComposition.Bmi(80, 0));
    }

    [Theory]
    [InlineData(17.0, "Underweight")]
    [InlineData(22.0, "Normal")]
    [InlineData(27.5, "Overweight")]
    [InlineData(31.0, "Obese")]
    public void BmiCategory_ClassifiesByBand(double bmi, string expected)
    {
        Assert.Equal(expected, BodyComposition.BmiCategory(bmi));
    }

    [Fact]
    public void HealthyWeightRange_DerivesFromHealthyBmiBand()
    {
        // height 1.80m => 18.5*3.24 = 59.94 -> 59.9 ; 24.9*3.24 = 80.676 -> 80.7
        Assert.Equal(59.9, BodyComposition.HealthyWeightMinKg(180));
        Assert.Equal(80.7, BodyComposition.HealthyWeightMaxKg(180));
    }
}
