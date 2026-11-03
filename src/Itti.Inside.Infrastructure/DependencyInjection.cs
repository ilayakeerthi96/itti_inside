using Itti.Inside.Application.Abstractions;
using Itti.Inside.Application.Identity;
using Itti.Inside.Infrastructure.Content;
using Itti.Inside.Infrastructure.Identity;
using Itti.Inside.Infrastructure.Persistence;
using Itti.Inside.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Itti.Inside.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddDbContext<PortalDbContext>(options => options.UseMySQL(
            configuration.GetConnectionString("Portal")
                ?? throw new InvalidOperationException("Set the database in ConnectionStrings:Portal.")));

        services.AddScoped<PortalDatabaseInitializer>();
        services.AddScoped<IPortalContentRepository, SqlPortalContentRepository>();
        services.AddScoped<IPortalContentEditor, SqlPortalContentEditor>();

        services.AddSingleton(_ => PortalSeedData.Load());
        services.AddScoped<INavigationRepository, SeedNavigationRepository>();

        services.AddCredentialValidator(configuration, environment);

        return services;
    }

    /// <summary>Call once at startup, before the site takes requests.</summary>
    public static async Task InitializePortalDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<PortalDatabaseInitializer>().InitializeAsync(cancellationToken);
    }

    private static void AddCredentialValidator(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var section = configuration.GetSection(PortalAuthenticationOptions.SectionName);

        services.AddOptions<PortalAuthenticationOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var provider = section[nameof(PortalAuthenticationOptions.Provider)] ?? PortalAuthenticationOptions.DemoProvider;

        switch (provider)
        {
            case PortalAuthenticationOptions.DemoProvider:
                if (environment.IsProduction())
                {
                    throw new InvalidOperationException(
                        "The Demo sign-in must not run in Production. Register a real ICredentialValidator and set Authentication:Provider.");
                }

                services.AddOptions<PortalAuthenticationOptions>()
                    .Validate(
                        options => options.Demo.UserId.Length > 0 && options.Demo.Password.Length > 0,
                        "The Demo sign-in needs Authentication:Demo:UserId and Authentication:Demo:Password.")
                    .Validate(
                        options => options.Admin.UserId.Length == 0 || options.Admin.Password.Length > 0,
                        "Set Authentication:Admin:Password, or clear Authentication:Admin:UserId to switch the admin account off.");

                services.AddScoped<ICredentialValidator, DemoCredentialValidator>();
                break;

            default:
                throw new InvalidOperationException($"Unknown Authentication:Provider '{provider}'.");
        }
    }
}
