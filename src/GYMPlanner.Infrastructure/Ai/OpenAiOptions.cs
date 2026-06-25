namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Bound from the "OpenAi" config section. Works with any OpenAI-compatible
/// chat-completions endpoint — Google Gemini (default), Groq, OpenRouter, etc.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// Base URL up to (but not including) /chat/completions. Leave empty to use the
    /// per-provider default chosen from ProgramGenerator:Provider (Gemini/Groq/OpenRouter/OpenAI).
    /// </summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>API key (sent as a Bearer token). Free keys: Gemini → aistudio.google.com, Groq → console.groq.com.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Model id. Leave empty to use the per-provider default.</summary>
    public string Model { get; set; } = "";
}
