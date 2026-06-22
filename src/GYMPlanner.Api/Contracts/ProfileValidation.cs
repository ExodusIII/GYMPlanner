using GYMPlanner.Domain;

namespace GYMPlanner.Api.Contracts;

/// <summary>Server-side range checks mirroring the client form. Returns an error message, or null if valid.</summary>
public static class ProfileValidation
{
    public static string? Validate(ClientProfile profile)
    {
        if (profile is null) return "Profile is required.";
        if (profile.Age is < 10 or > 100) return "Age must be between 10 and 100.";
        if (profile.HeightCm is < 100 or > 250) return "Height must be between 100 and 250 cm.";
        if (profile.WeightKg is < 30 or > 300) return "Weight must be between 30 and 300 kg.";
        if (profile.DaysPerWeek is < 1 or > 7) return "Days per week must be between 1 and 7.";
        if (profile.MinutesPerSession is < 10 or > 180) return "Minutes per session must be between 10 and 180.";
        if (profile.BodyFatPercent is < 0 or > 70) return "Body fat percent must be between 0 and 70.";
        return null;
    }
}
