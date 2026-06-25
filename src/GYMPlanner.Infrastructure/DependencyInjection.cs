using GYMPlanner.Application.Programs;
using GYMPlanner.Infrastructure.Ai;
using GYMPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GYMPlanner.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence (EF Core + PostgreSQL via Npgsql), the program
    /// repository, the Claude generator, and the program orchestration service.
    /// Identity and JWT auth are wired in the API host, which owns auth config.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing connection string 'Default' (PostgreSQL).");
        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IProgramRepository, ProgramRepository>();

        // Choose the AI program generator via ProgramGenerator:Provider:
        //   "Claude"  – paid Anthropic API (default)
        //   "Ollama"  – free, local
        //   "Gemini" / "Groq" / "OpenRouter" / "OpenAI" – any OpenAI-compatible API
        services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
        services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAi"));

        var provider = configuration["ProgramGenerator:Provider"];
        string[] openAiCompatible = ["OpenAI", "Gemini", "Groq", "OpenRouter"];

        if (string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IProgramGenerator, OllamaProgramGenerator>(c => c.Timeout = TimeSpan.FromMinutes(10));
        }
        else if (provider is not null && openAiCompatible.Contains(provider, StringComparer.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IProgramGenerator, OpenAiCompatibleProgramGenerator>(c => c.Timeout = TimeSpan.FromMinutes(5));
            // Fill the right endpoint + model for the chosen provider unless explicitly set.
            services.PostConfigure<OpenAiOptions>(o =>
            {
                var (baseUrl, model) = OpenAiDefaultsFor(provider);
                if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = baseUrl;
                if (string.IsNullOrWhiteSpace(o.Model)) o.Model = model;
            });
        }
        else
        {
            services.AddScoped<IProgramGenerator, ClaudeProgramGenerator>();
        }

        services.AddScoped<ProgramService>();

        return services;
    }

    private static (string BaseUrl, string Model) OpenAiDefaultsFor(string provider) => provider.ToLowerInvariant() switch
    {
        "groq" => ("https://api.groq.com/openai/v1", "llama-3.3-70b-versatile"),
        "openrouter" => ("https://openrouter.ai/api/v1", "meta-llama/llama-3.3-70b-instruct"),
        "openai" => ("https://api.openai.com/v1", "gpt-4o-mini"),
        _ => ("https://generativelanguage.googleapis.com/v1beta/openai", "gemini-2.0-flash") // Gemini
    };
}
