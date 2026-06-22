namespace GYMPlanner.Infrastructure.Ai;

/// <summary>Bound from the "Claude" configuration section.</summary>
public sealed class ClaudeOptions
{
    /// <summary>
    /// API key. If left null/empty the client falls back to the
    /// ANTHROPIC_API_KEY environment variable (the recommended way to supply it).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Model id. Defaults to the most capable Opus model. Switch to
    /// "claude-sonnet-4-6" or "claude-haiku-4-5" here to trade quality for cost.
    /// </summary>
    public string Model { get; set; } = "claude-opus-4-8";
}
