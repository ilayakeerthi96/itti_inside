using Itti.Inside.Application.Identity;
using Itti.Inside.Domain.Navigation;
using Itti.Inside.Infrastructure;
using Itti.Inside.Infrastructure.Identity;
using Itti.Inside.Infrastructure.Seed;
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
            .Where(path => path is not "/" and not "/applications" && path.StartsWith('/'));

        Assert.All(links, path => Assert.Contains(Seed.Pages, page => page.Path == path));
    }

    [Fact]
    public void Menu_links_to_other_sites_use_https()
    {
        var links = Seed.Menu
            .SelectMany(item => (item.Children ?? []).Append(item))
            .Select(item => item.Path)
            .OfType<string>()
            .Where(path => !path.StartsWith('/'));

        Assert.NotEmpty(links);
        Assert.All(links, path => Assert.StartsWith("https://", path, StringComparison.Ordinal));
    }

    [Fact]
    public void Every_application_tile_has_a_page_behind_it()
    {
        Assert.All(Seed.Applications, application => Assert.Contains(Seed.Pages, page => page.Path == application.Path));
    }
}

public sealed class NavigationItemTests
{
    [Theory]
    [InlineData("https://inside.itti.com/Logout.asp", true)]
    [InlineData("http://inside.itti.com/", true)]
    [InlineData("/hr/travel-desk", false)]
    [InlineData("/", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData(null, false)]
    public void Knows_which_links_go_to_another_site(string? path, bool isExternal)
    {
        Assert.Equal(isExternal, new NavigationItem("Item", path, []).IsExternal);
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
