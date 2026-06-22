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
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));

        services.AddScoped<IProgramRepository, ProgramRepository>();

        services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
        services.AddScoped<IProgramGenerator, ClaudeProgramGenerator>();

        services.AddScoped<ProgramService>();

        return services;
    }
}
