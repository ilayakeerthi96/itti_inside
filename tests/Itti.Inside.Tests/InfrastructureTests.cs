using Itti.Inside.Application.Identity;
using Itti.Inside.Infrastructure;
using Itti.Inside.Infrastructure.Identity;
using Itti.Inside.Infrastructure.Persistence;
using Itti.Inside.Infrastructure.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;

namespace Itti.Inside.Tests;

public sealed class SeedDataTests
{
    private static readonly PortalSeedData Seed = PortalSeedData.Load();

    [Fact]
    public void Every_menu_link_has_a_page_behind_it()
    {
        var links = Seed.Menu
            .SelectMany(item => (item.Children ?? []).Append(item))
            .Select(item => item.Path)
            .OfType<string>()
            .Where(path => path is not "/" and not "/applications");

        Assert.All(links, path => Assert.Contains(Seed.Pages, page => page.Path == path));
    }

    [Fact]
    public void Every_application_tile_has_a_page_behind_it()
    {
        Assert.All(Seed.Applications, application => Assert.Contains(Seed.Pages, page => page.Path == application.Path));
    }
}

public sealed class DemoCredentialValidatorTests
{
    private static readonly DemoCredentialValidator Validator = new(Options.Create(new PortalAuthenticationOptions
    {
        Demo = new DemoUserOptions { UserId = "demo.user", Password = "right", DisplayName = "Demo User" },
        Admin = new DemoUserOptions { UserId = "portal.admin", Password = "admin-right", DisplayName = "Portal Admin" },
    }));

    [Theory]
    [InlineData("demo.user")]
    [InlineData("DEMO.USER")]
    [InlineData("  demo.user  ")]
    public async Task Accepts_the_configured_user(string userId)
    {
        var user = await Validator.ValidateAsync(userId, "right", CancellationToken.None);

        Assert.Equal("Demo User", user?.DisplayName);
    }

    [Fact]
    public async Task Employee_account_is_not_an_admin()
    {
        var user = await Validator.ValidateAsync("demo.user", "right", CancellationToken.None);

        Assert.DoesNotContain(PortalRoles.Admin, user!.Roles);
    }

    [Fact]
    public async Task Admin_account_is_only_an_admin()
    {
        var user = await Validator.ValidateAsync("portal.admin", "admin-right", CancellationToken.None);

        Assert.Equal([PortalRoles.Admin], user!.Roles);
    }

    [Theory]
    [InlineData("demo.user", "wrong")]
    [InlineData("demo.user", "RIGHT")]
    [InlineData("demo.user", "admin-right")]
    [InlineData("portal.admin", "right")]
    [InlineData("other", "right")]
    public async Task Rejects_everything_else(string userId, string password)
    {
        Assert.Null(await Validator.ValidateAsync(userId, password, CancellationToken.None));
    }

    [Fact]
    public async Task Admin_account_without_a_user_id_is_switched_off()
    {
        var validator = new DemoCredentialValidator(Options.Create(new PortalAuthenticationOptions
        {
            Admin = new DemoUserOptions { UserId = string.Empty, Password = string.Empty },
        }));

        Assert.Null(await validator.ValidateAsync(string.Empty, string.Empty, CancellationToken.None));
    }
}

public sealed class DateMappingTests
{
    // The tests run on SQLite, which reads dates either way. MySQL needs the converter, or every page with a date fails.
    [Fact]
    public void Every_date_column_goes_through_the_converter_mysql_needs()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var context = new PortalDbContext(new DbContextOptionsBuilder<PortalDbContext>().UseSqlite(database).Options);

        var dates = context.Model.GetEntityTypes()
            .SelectMany(entity => entity.GetProperties())
            .Where(property => property.ClrType == typeof(DateOnly))
            .ToArray();

        Assert.NotEmpty(dates);
        Assert.All(dates, property => Assert.IsType<DateOnlyConverter>(property.GetValueConverter()));
    }

    [Fact]
    public void Dates_survive_the_conversion_unchanged()
    {
        var converter = new DateOnlyConverter();
        var date = new DateOnly(2026, 10, 9);

        var stored = (DateTime)converter.ConvertToProvider(date)!;

        Assert.Equal(date, (DateOnly)converter.ConvertFromProvider(stored)!);
    }
}

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void Demo_sign_in_refuses_to_start_in_production()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Register(Environments.Production, "Demo"));

        Assert.Contains("must not run in Production", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    public void Demo_sign_in_is_allowed_outside_production(string environment)
    {
        var exception = Record.Exception(() => Register(environment, "Demo"));

        Assert.Null(exception);
    }

    [Fact]
    public void Unknown_provider_fails_at_startup()
    {
        Assert.Throws<InvalidOperationException>(() => Register(Environments.Staging, "Typo"));
    }

    private static void Register(string environmentName, string provider)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Authentication:Provider"] = provider })
            .Build();

        new ServiceCollection().AddInfrastructure(configuration, new HostingEnvironment { EnvironmentName = environmentName });
    }
}
