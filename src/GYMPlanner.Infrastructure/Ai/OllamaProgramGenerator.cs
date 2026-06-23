using System.Net.Http.Json;
using System.Text.Json;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.Extensions.Options;

namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Free, local alternative to <see cref="ClaudeProgramGenerator"/>: calls an
/// Ollama server's chat endpoint with the same grounded prompt, constraining the
/// output to the program JSON schema via Ollama's structured-output `format`.
/// </summary>
internal sealed class OllamaProgramGenerator(HttpClient http, IOptions<OllamaOptions> options) : IProgramGenerator
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<WeeklyProgram> GenerateAsync(
        ClientProfile profile,
        CalculatedMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        var url = _options.BaseUrl.TrimEnd('/') + "/api/chat";

        var requestBody = new
        {
            model = _options.Model,
            stream = false,
            format = ProgramPrompt.BuildSchemaElement(),
            messages = new[]
            {
                new { role = "system", content = ProgramPrompt.System },
                new { role = "user", content = ProgramPrompt.BuildUserMessage(profile, metrics) }
            },
            options = new { temperature = 0.7 }
        };

        HttpResponseMessage response;
        try
        {
            response = await http.PostAsJsonAsync(url, requestBody, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Ollama is not reachable at {_options.BaseUrl}. Make sure it is running and the model is pulled " +
                $"(`ollama pull {_options.Model}`).", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama request failed ({(int)response.StatusCode}). Is the model \"{_options.Model}\" pulled? {error}");
        }

        var chat = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var content = chat?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Ollama returned no program content.");

        return JsonSerializer.Deserialize<WeeklyProgram>(content, AppJson.Options)
            ?? throw new InvalidOperationException("Failed to parse the generated program JSON from Ollama.");
    }

    private sealed record OllamaChatResponse(OllamaMessage? Message);
    private sealed record OllamaMessage(string? Role, string? Content);
}
