using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.Extensions.Options;

namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Generates the weekly program via any OpenAI-compatible chat-completions API
/// (Google Gemini, Groq, OpenRouter, …). Uses the same grounded prompt and
/// constrains output with <c>response_format: json_schema</c> so it deserializes
/// straight into <see cref="WeeklyProgram"/>. The key stays on the backend.
/// </summary>
internal sealed class OpenAiCompatibleProgramGenerator(HttpClient http, IOptions<OpenAiOptions> options) : IProgramGenerator
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<WeeklyProgram> GenerateAsync(
        ClientProfile profile,
        CalculatedMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "AI API key is not configured. Set OpenAi:ApiKey (e.g. a free Gemini key from https://aistudio.google.com).");

        var url = _options.BaseUrl.TrimEnd('/') + "/chat/completions";

        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = ProgramPrompt.System },
                new { role = "user", content = ProgramPrompt.BuildUserMessage(profile, metrics) }
            },
            // json_object is supported across providers (Gemini, Groq, OpenRouter, …);
            // the exact shape is enforced by the prompt. (json_schema isn't supported
            // by all Groq models.)
            response_format = new { type = "json_object" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody, options: AppJson.Options)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"The AI provider at {_options.BaseUrl} is not reachable.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"AI request failed ({(int)response.StatusCode}): {error}");
        }

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletion>(AppJson.Options, cancellationToken);
        var content = completion?.Choices is { Count: > 0 } choices ? choices[0].Message?.Content : null;
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("The AI provider returned no program content.");

        return JsonSerializer.Deserialize<WeeklyProgram>(content, AppJson.Options)
            ?? throw new InvalidOperationException("Failed to parse the generated program JSON.");
    }

    private sealed record ChatCompletion(List<Choice>? Choices);
    private sealed record Choice(ChatMessage? Message);
    private sealed record ChatMessage(string? Role, string? Content);
}
