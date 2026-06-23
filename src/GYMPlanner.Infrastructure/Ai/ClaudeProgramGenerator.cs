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
            System = ProgramPrompt.System,
            OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = BuildSchema() } },
            Messages = [new() { Role = Role.User, Content = ProgramPrompt.BuildUserMessage(profile, metrics) }]
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

    private static Dictionary<string, JsonElement> BuildSchema()
    {
        var dict = new Dictionary<string, JsonElement>();
        foreach (var property in ProgramPrompt.BuildSchemaElement().EnumerateObject())
            dict[property.Name] = property.Value.Clone();
        return dict;
    }
}
