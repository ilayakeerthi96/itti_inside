using System.Globalization;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itti.Inside.Tests;

/// <summary>
/// Runs the real application in memory with test-only sign-in details. Each factory gets
/// its own empty in-memory database, so the tests need no database server.
/// </summary>
public class PortalWebFactory : WebApplicationFactory<Program>
{
    public const string UserId = "test.user";
    public const string Password = "test-only-password";
    public const string AdminId = "test.admin";
    public const string AdminPassword = "test-only-admin-password";

    private readonly SqliteConnection _database = new("Data Source=:memory:");

    protected virtual int SignInAttemptsPerMinute => 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        builder.UseSetting("Authentication:Provider", "Demo");
        builder.UseSetting("Authentication:Demo:UserId", UserId);
        builder.UseSetting("Authentication:Demo:Password", Password);
        builder.UseSetting("Authentication:Demo:DisplayName", "Test User");
        builder.UseSetting("Authentication:Admin:UserId", AdminId);
        builder.UseSetting("Authentication:Admin:Password", AdminPassword);
        builder.UseSetting("Authentication:Admin:DisplayName", "Test Admin");
        builder.UseSetting("RateLimiting:SignInAttemptsPerMinute", SignInAttemptsPerMinute.ToString(CultureInfo.InvariantCulture));
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");

        _database.Open();

        var options = new DbContextOptionsBuilder<PortalDbContext>().UseSqlite(_database).Options;

        using (var context = new PortalDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PortalDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<PortalDbContext>>();
            services.AddDbContext<PortalDbContext>(sqlite => sqlite.UseSqlite(_database));
        });
    }

    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true,
    });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _database.Dispose();
        }
    }
}

/// <summary>Same application with a tight limit, for testing the sign-in rate limit.</summary>
public sealed class StrictRateLimitWebFactory : PortalWebFactory
{
    protected override int SignInAttemptsPerMinute => 2;
}
