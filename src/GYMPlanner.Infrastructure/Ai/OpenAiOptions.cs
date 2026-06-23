namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Bound from the "OpenAi" config section. Works with any OpenAI-compatible
/// chat-completions endpoint — Google Gemini (default), Groq, OpenRouter, etc.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>Base URL up to (but not including) /chat/completions. Defaults to Google Gemini.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai";

    /// <summary>API key (sent as a Bearer token). Get a free Gemini key at https://aistudio.google.com.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Model id, e.g. "gemini-2.0-flash" (Gemini) or "llama-3.3-70b-versatile" (Groq).</summary>
    public string Model { get; set; } = "gemini-2.0-flash";
}
