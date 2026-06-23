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

        // Choose the AI program generator: "Claude" (default, paid API) or
        // "Ollama" (free, local). Set ProgramGenerator:Provider in config/env.
        services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
        services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));

        var provider = configuration["ProgramGenerator:Provider"];
        if (string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase))
            services.AddHttpClient<IProgramGenerator, OllamaProgramGenerator>(c => c.Timeout = TimeSpan.FromMinutes(10));
        else
            services.AddScoped<IProgramGenerator, ClaudeProgramGenerator>();

        services.AddScoped<ProgramService>();

        return services;
    }
}
