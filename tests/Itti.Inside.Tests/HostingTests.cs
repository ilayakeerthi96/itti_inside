using System.Net;
using System.Text.RegularExpressions;

namespace Itti.Inside.Tests;

/// <summary>The site with only the admin account configured, as the IIS setup now does.</summary>
public sealed class AdminOnlyTests(AdminOnlyWebFactory factory) : IClassFixture<AdminOnlyWebFactory>
{
    [Fact]
    public async Task Landing_page_opens_without_an_employee_account()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_still_signs_in_to_the_master_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.AdminId, PortalWebFactory.AdminPassword);

        Assert.Equal("/master", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Employee_account_is_switched_off()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("incorrect", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}

/// <summary>The CEO's Desk "More" button, whose address comes from configuration.</summary>
public sealed class CeoDeskLinkTests
{
    [Fact]
    public async Task Without_an_address_the_button_opens_this_sites_page()
    {
        using var factory = new CeoDeskLinkWebFactory(string.Empty);
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        Assert.Equal("/ceo-desk", PageLinks.Find(html, "More").Href);
    }

    [Fact]
    public void An_address_that_is_not_a_web_page_stops_the_site_from_starting()
    {
        using var factory = new CeoDeskLinkWebFactory("javascript:alert(1)");

        var error = Assert.ThrowsAny<Exception>(() => factory.CreateBrowser());

        Assert.Contains("Links:CeoDesk", error.ToString(), StringComparison.Ordinal);
    }
}

/// <summary>The site as an IIS application under the intranet site, at /inside.</summary>
public sealed partial class SubPathTests(SubPathWebFactory factory) : IClassFixture<SubPathWebFactory>
{
    private const string PathBase = SubPathWebFactory.PathBase;

    [Fact]
    public async Task Landing_page_and_its_files_stay_under_the_sub_path()
    {
        using var client = factory.CreateSubPathBrowser();

        using var home = await client.GetAsync(new Uri($"{PathBase}/", UriKind.Relative));
        using var css = await client.GetAsync(new Uri($"{PathBase}/css/portal.css", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
        AssertLinksStayUnderThePathBase(await home.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Master_page_sends_a_signed_out_visitor_to_the_sign_in_page_under_the_sub_path()
    {
        using var client = factory.CreateSubPathBrowser();

        using var response = await client.GetAsync(new Uri($"{PathBase}/master/birthdays", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith($"{PathBase}/account/login", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Admin_signs_in_and_works_under_the_sub_path()
    {
        using var client = factory.CreateSubPathBrowser();

        using var signIn = await SignInAsAdminAsync(client, returnUrl: string.Empty);
        var master = await client.GetStringAsync(new Uri($"{PathBase}/master/hero-slides", UriKind.Relative));

        Assert.Equal($"{PathBase}/master", signIn.Headers.Location?.OriginalString);
        Assert.All(
            signIn.Headers.GetValues("Set-Cookie"),
            cookie => Assert.Contains($"path={PathBase}", cookie, StringComparison.OrdinalIgnoreCase));
        AssertLinksStayUnderThePathBase(master);
    }

    [Fact]
    public async Task Admin_returns_to_the_master_section_they_asked_for_under_the_sub_path()
    {
        using var client = factory.CreateSubPathBrowser();

        using var signIn = await SignInAsAdminAsync(client, $"{PathBase}/master/birthdays");

        Assert.Equal($"{PathBase}/master/birthdays", signIn.Headers.Location?.OriginalString);
    }

    private static async Task<HttpResponseMessage> SignInAsAdminAsync(HttpClient client, string returnUrl)
    {
        var token = await client.GetAntiforgeryTokenAsync($"{PathBase}/account/login");

        return await client.PostAsync(new Uri($"{PathBase}/account/login", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["UserId"] = PortalWebFactory.AdminId,
            ["Password"] = PortalWebFactory.AdminPassword,
            ["ReturnUrl"] = returnUrl,
        }));
    }

    // Every link to this site must include /inside, or it would land on the intranet site above it.
    private static void AssertLinksStayUnderThePathBase(string html)
    {
        var outside = SiteLink().Matches(html)
            .Select(match => match.Groups[1].Value)
            .Where(link => link != PathBase && !link.StartsWith($"{PathBase}/", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(outside.Count == 0, $"Links that leave {PathBase}: {string.Join(", ", outside)}");
    }

    [GeneratedRegex("(?:href|src|action|formaction)=\"(/[^\"]*)\"")]
    private static partial Regex SiteLink();
}
