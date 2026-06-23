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
        You write safe, practical weekly training and nutrition programs.

        Rules:
        - Treat the provided computed numbers as ground truth. The nutrition section MUST copy the
          given calorie target and protein/carb/fat grams exactly.
        - Produce exactly the number of training days the client can train, using the recommended split.
        - Respect the client's available equipment, experience level, and any injuries
          (avoid contraindicated movements for the listed injuries).
        - Keep exercise selection realistic and progressively sensible.
        - This is general fitness guidance, not medical advice.
        Return only data that conforms to the provided JSON schema.
        """;

    public static string BuildUserMessage(ClientProfile profile, CalculatedMetrics metrics)
    {
        var profileJson = JsonSerializer.Serialize(profile, AppJson.Options);
        var metricsJson = JsonSerializer.Serialize(metrics, AppJson.Options);

        return $"""
            Create a weekly fitness and nutrition program for this client.

            CLIENT PROFILE:
            {profileJson}

            COMPUTED TARGETS (ground truth — copy the nutrition numbers exactly):
            {metricsJson}

            Build {profile.DaysPerWeek} training day(s) using the "{metrics.Training.Split}" split,
            roughly {metrics.Training.WeeklySetsPerMuscleGroup} weekly working sets per major muscle group,
            each session about {profile.MinutesPerSession} minutes, for "{profile.Equipment}" equipment access.
            The nutrition section must use dailyCalories={metrics.CalorieTarget},
            proteinGrams={metrics.Macros.ProteinGrams}, carbGrams={metrics.Macros.CarbGrams},
            fatGrams={metrics.Macros.FatGrams}.
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
