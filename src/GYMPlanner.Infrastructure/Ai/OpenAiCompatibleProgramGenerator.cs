using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GYMPlanner.Application.Programs;
using GYMPlanner.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GYMPlanner.Infrastructure.Ai;

/// <summary>
/// Generates the weekly program via any OpenAI-compatible chat-completions API
/// (Google Gemini, Groq, OpenRouter, …). Uses the same grounded prompt and asks
/// for a JSON object (<c>response_format: json_object</c>) shaped by the prompt,
/// then deserializes into <see cref="WeeklyProgram"/>. The key stays on the backend.
/// The outgoing request and the response are logged (without the API key).
/// </summary>
internal sealed class OpenAiCompatibleProgramGenerator(
    HttpClient http,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiCompatibleProgramGenerator> logger) : IProgramGenerator
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<WeeklyProgram> GenerateAsync(
        ClientProfile profile,
        CalculatedMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "AI API key is not configured. Set OpenAi:ApiKey / AI_API_KEY (e.g. a free Groq key from https://console.groq.com).");

        var url = _options.BaseUrl.TrimEnd('/') + "/chat/completions";

        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = ProgramPrompt.System },
                new { role = "user", content = ProgramPrompt.BuildUserMessage(profile, metrics) }
            },
            response_format = new { type = "json_object" }
        };

        var bodyJson = JsonSerializer.Serialize(requestBody, AppJson.Options);

        // Log the exact request we send (the Authorization header / API key is NOT logged).
        logger.LogInformation("AI request → POST {Url} (model {Model})\n{Body}", url, _options.Model, bodyJson);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
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

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("AI response ← {Status}\n{Body}", (int)response.StatusCode, responseText);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"AI request failed ({(int)response.StatusCode}): {responseText}");

        var completion = JsonSerializer.Deserialize<ChatCompletion>(responseText, AppJson.Options);
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
