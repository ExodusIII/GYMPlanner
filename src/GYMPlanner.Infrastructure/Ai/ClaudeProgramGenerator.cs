using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.Extensions.Options;

namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Hybrid generator: the deterministic metrics are passed in as ground truth and
/// Claude writes the human-friendly weekly plan. Output is constrained to a JSON
/// schema so it deserializes straight into <see cref="WeeklyProgram"/>. The API
/// key never leaves the backend.
/// </summary>
internal sealed class ClaudeProgramGenerator(IOptions<ClaudeOptions> options) : IProgramGenerator
{
    private readonly ClaudeOptions _options = options.Value;

    public async Task<WeeklyProgram> GenerateAsync(
        ClientProfile profile,
        CalculatedMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();

        var parameters = new MessageCreateParams
        {
            Model = _options.Model,
            MaxTokens = 16000,
            Thinking = new ThinkingConfigAdaptive(),
            System = SystemPrompt,
            OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = BuildSchema() } },
            Messages = [new() { Role = Role.User, Content = BuildUserMessage(profile, metrics) }]
        };

        // Stream so a large structured output doesn't risk an HTTP timeout; we only
        // accumulate text deltas (the JSON), ignoring adaptive-thinking deltas.
        var json = new StringBuilder();
        await foreach (var streamEvent in client.Messages.CreateStreaming(parameters).WithCancellation(cancellationToken))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
                json.Append(text.Text);
        }

        var raw = json.ToString();
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("The model returned no program content (possibly a refusal).");

        return JsonSerializer.Deserialize<WeeklyProgram>(raw, AppJson.Options)
            ?? throw new InvalidOperationException("Failed to parse the generated program JSON.");
    }

    private AnthropicClient CreateClient()
    {
        var key = string.IsNullOrWhiteSpace(_options.ApiKey)
            ? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            : _options.ApiKey;

        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(
                "Claude API key is not configured. Set the ANTHROPIC_API_KEY environment variable " +
                "or Claude:ApiKey in configuration to generate AI programs.");

        return new AnthropicClient { ApiKey = key };
    }

    private const string SystemPrompt = """
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

    private static string BuildUserMessage(ClientProfile profile, CalculatedMetrics metrics)
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
    /// JSON Schema matching <see cref="WeeklyProgram"/>. Every object sets
    /// additionalProperties:false and uses only schema features structured
    /// outputs supports (no numeric/length constraints).
    /// </summary>
    private static Dictionary<string, JsonElement> BuildSchema()
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

        // SerializeToElement with default options keeps the property names verbatim
        // (the identifiers above are already the exact schema keys we want).
        using var doc = JsonSerializer.SerializeToDocument(schema);
        var dict = new Dictionary<string, JsonElement>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            dict[prop.Name] = prop.Value.Clone();
        return dict;
    }
}
