namespace GYMPlanner.Domain;

public enum Sex
{
    Male,
    Female,
    Other
}

public enum Goal
{
    LoseFat,
    BuildMuscle,
    Maintain,
    Recomp,
    Strength,
    Endurance
}

public enum ActivityLevel
{
    /// <summary>Little or no exercise, desk job.</summary>
    Sedentary,

    /// <summary>Light exercise 1-3 days/week.</summary>
    Light,

    /// <summary>Moderate exercise 3-5 days/week.</summary>
    Moderate,

    /// <summary>Hard exercise 6-7 days/week.</summary>
    Active,

    /// <summary>Very hard exercise / physical job / training twice a day.</summary>
    VeryActive
}

public enum ExperienceLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum Equipment
{
    /// <summary>Full commercial gym access.</summary>
    Gym,

    /// <summary>Home setup (some dumbbells / bands / bodyweight).</summary>
    Home,

    /// <summary>Minimal or bodyweight only.</summary>
    Minimal
}
