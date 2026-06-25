using System.Text.Json;
using GYMPlanner.Domain;

namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Shared prompt + output-schema for the weekly program, used by every
/// <see cref="GYMPlanner.Application.Programs.IProgramGenerator"/> implementation
/// (Claude, Ollama, …) so they stay consistent.
/// </summary>
internal static class ProgramPrompt
{
    public const string System = """
        You are an expert strength-and-conditioning coach and registered dietitian.
        You output a structured weekly program as JSON. Be concrete, consistent, and complete.

        Hard requirements (every time):
        - The "days" array MUST contain EXACTLY the requested number of training days — no more, no fewer.
        - Each day: a short "focus" label (e.g. "Upper Body", "Lower Body", "Push", "Pull", "Legs",
          "Full Body") consistent with the requested split, and 4 to 6 exercises.
        - Each exercise: a real exercise "name"; "sets" as an integer from 3 to 5; "reps" as a RANGE
          string such as "8-12" or "10-15" (NEVER a single letter, a number alone, or empty); and a
          short practical "notes" tip (max ~12 words, never empty).
        - The nutrition section MUST copy the provided calorie and macro numbers EXACTLY, and include
          4 to 6 concrete "mealSuggestions" (real meals, not placeholders).
        - "notes": one or two sentences of overall guidance.
        - Respect the client's equipment, experience, and injuries (avoid contraindicated movements).
        - This is general fitness guidance, not medical advice.
        Never output placeholder, empty, or single-character values.

        Respond with a SINGLE JSON object using EXACTLY these camelCase keys and no others:
        {"days":[{"day":"Day 1","focus":"Upper Body","exercises":[{"name":"Bench Press","sets":4,"reps":"8-12","notes":"Keep core tight"}]}],"nutrition":{"dailyCalories":0,"proteinGrams":0,"carbGrams":0,"fatGrams":0,"mealSuggestions":["Oats with berries"]},"notes":"Progress weekly."}
        """;

    public static string BuildUserMessage(ClientProfile profile, CalculatedMetrics metrics)
    {
        var profileJson = JsonSerializer.Serialize(profile, AppJson.Options);
        var injuries = profile.Injuries.Count > 0 ? string.Join(", ", profile.Injuries) : "none";

        return $"""
            Create a {profile.DaysPerWeek}-day weekly training and nutrition program for this client.
            Return EXACTLY {profile.DaysPerWeek} entries in the "days" array — one per training day.

            Split: "{metrics.Training.Split}" — distribute the focus across the {profile.DaysPerWeek} days accordingly.
            Target ~{metrics.Training.WeeklySetsPerMuscleGroup} working sets per major muscle group per week.
            Session length: ~{profile.MinutesPerSession} minutes. Equipment: "{profile.Equipment}".
            Experience: "{profile.Experience}". Injuries to work around: {injuries}.

            Nutrition — copy these numbers EXACTLY:
            dailyCalories={metrics.CalorieTarget}, proteinGrams={metrics.Macros.ProteinGrams},
            carbGrams={metrics.Macros.CarbGrams}, fatGrams={metrics.Macros.FatGrams}.

            CLIENT PROFILE (JSON):
            {profileJson}
            """;
    }

    /// <summary>
    /// JSON Schema for <see cref="GYMPlanner.Application.Programs.WeeklyProgram"/>.
    /// Returned as a <see cref="JsonElement"/> so Claude can split it into its
    /// schema dictionary and Ollama can embed it directly in the request body.
    /// </summary>
    public static JsonElement BuildSchemaElement()
    {
        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "days", "nutrition", "notes" },
            properties = new
            {
                days = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "day", "focus", "exercises" },
                        properties = new
                        {
                            day = new { type = "string" },
                            focus = new { type = "string" },
                            exercises = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    required = new[] { "name", "sets", "reps", "notes" },
                                    properties = new
                                    {
                                        name = new { type = "string" },
                                        sets = new { type = "integer" },
                                        reps = new { type = "string" },
                                        notes = new { type = "string" }
                                    }
                                }
                            }
                        }
                    }
                },
                nutrition = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "dailyCalories", "proteinGrams", "carbGrams", "fatGrams", "mealSuggestions" },
                    properties = new
                    {
                        dailyCalories = new { type = "integer" },
                        proteinGrams = new { type = "integer" },
                        carbGrams = new { type = "integer" },
                        fatGrams = new { type = "integer" },
                        mealSuggestions = new { type = "array", items = new { type = "string" } }
                    }
                },
                notes = new { type = "string" }
            }
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
