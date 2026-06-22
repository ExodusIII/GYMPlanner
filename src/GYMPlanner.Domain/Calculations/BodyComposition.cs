namespace GYMPlanner.Domain.Calculations;

/// <summary>Body Mass Index and the healthy-weight range derived from it.</summary>
public static class BodyComposition
{
    // Healthy BMI band used for the recommended weight range.
    private const double HealthyBmiMin = 18.5;
    private const double HealthyBmiMax = 24.9;

    /// <summary>BMI = weight(kg) / height(m)^2, rounded to one decimal place.</summary>
    public static double Bmi(double weightKg, double heightCm)
    {
        if (heightCm <= 0)
            throw new ArgumentOutOfRangeException(nameof(heightCm), "Height must be positive.");

        var heightM = heightCm / 100.0;
        return Math.Round(weightKg / (heightM * heightM), 1);
    }

    public static string BmiCategory(double bmi) => bmi switch
    {
        < 18.5 => "Underweight",
        < 25.0 => "Normal",
        < 30.0 => "Overweight",
        _ => "Obese"
    };

    /// <summary>Weight (kg) at the bottom of the healthy BMI band for this height.</summary>
    public static double HealthyWeightMinKg(double heightCm)
    {
        var heightM = heightCm / 100.0;
        return Math.Round(HealthyBmiMin * heightM * heightM, 1);
    }

    /// <summary>Weight (kg) at the top of the healthy BMI band for this height.</summary>
    public static double HealthyWeightMaxKg(double heightCm)
    {
        var heightM = heightCm / 100.0;
        return Math.Round(HealthyBmiMax * heightM * heightM, 1);
    }
}
