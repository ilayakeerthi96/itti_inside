using System.Net;

namespace Itti.Inside.Tests;

public sealed class SignInTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task Anonymous_visitor_sees_the_landing_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sign_in_page_is_for_admins()
    {
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/account/login", UriKind.Relative));

        Assert.Contains("Admin Sign In", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Correct_employee_credentials_lead_back_to_the_landing_page()
    {
        using var client = factory.CreateBrowser();

        using var signIn = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password);
        using var home = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, signIn.StatusCode);
        Assert.Equal("/", signIn.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
    }

    [Fact]
    public async Task Sign_in_cookie_is_protected_from_scripts_and_plain_http()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password);

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith(".Itti.Inside.Auth=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(PortalWebFactory.UserId, "wrong-password")]
    [InlineData("someone.else", PortalWebFactory.Password)]
    public async Task Wrong_credentials_show_one_generic_message(string userId, string password)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(userId, password);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("incorrect", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Password_is_never_echoed_back_into_the_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, "my-secret-attempt");
        var html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("my-secret-attempt", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Sign_in_without_an_antiforgery_token_is_rejected()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostAsync(new Uri("/account/login", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["UserId"] = PortalWebFactory.UserId,
            ["Password"] = PortalWebFactory.Password,
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("https://evil.example/phish")]
    [InlineData("//evil.example/phish")]
    public async Task Sign_in_ignores_a_return_url_on_another_site(string returnUrl)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password, returnUrl);

        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Sign_in_honours_a_return_url_on_this_site()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password, "/hr/travel-desk");

        Assert.Equal("/hr/travel-desk", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_ends_the_admin_session()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var logout = await client.PostFormAsync("/master/ceo-desk", "/account/logout");
        using var afterLogout = await client.GetAsync(new Uri("/master/ceo-desk", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.StartsWith("/account/login", afterLogout.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
    }
}

public sealed class SignInRateLimitTests(StrictRateLimitWebFactory factory) : IClassFixture<StrictRateLimitWebFactory>
{
    [Fact]
    public async Task Repeated_sign_in_attempts_are_blocked()
    {
        using var client = factory.CreateBrowser();

        using var first = await client.PostSignInAsync("nobody", "wrong");
        using var second = await client.PostSignInAsync("nobody", "wrong");
        using var third = await client.PostSignInAsync("nobody", "wrong");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }
}
