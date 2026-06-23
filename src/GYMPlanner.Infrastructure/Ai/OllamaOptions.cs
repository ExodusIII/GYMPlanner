namespace GYMPlanner.Infrastructure.Ai;

/// <summary>Bound from the "Ollama" configuration section. Free, local LLM provider.</summary>
public sealed class OllamaOptions
{
    /// <summary>Base URL of the Ollama server (default the local daemon).</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Model tag to use, e.g. "llama3.2", "qwen2.5". Must be pulled first (`ollama pull`).</summary>
    public string Model { get; set; } = "llama3.2";
}
