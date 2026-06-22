namespace GYMPlanner.Application.Programs;

/// <summary>
/// The AI-generated weekly program. Its shape mirrors the JSON schema the
/// generator constrains Claude's output to, so it deserializes cleanly and the
/// React client can render it without guesswork.
/// </summary>
public sealed record WeeklyProgram(
    IReadOnlyList<ProgramDay> Days,
    NutritionPlan Nutrition,
    string Notes);

public sealed record ProgramDay(
    string Day,
    string Focus,
    IReadOnlyList<Exercise> Exercises);

public sealed record Exercise(
    string Name,
    int Sets,
    string Reps,
    string Notes);

public sealed record NutritionPlan(
    int DailyCalories,
    int ProteinGrams,
    int CarbGrams,
    int FatGrams,
    IReadOnlyList<string> MealSuggestions);
