using System.Net;

namespace Itti.Inside.Tests;

public sealed class SecurityHeadersTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Theory]
    [InlineData("X-Content-Type-Options", "nosniff")]
    [InlineData("X-Frame-Options", "DENY")]
    [InlineData("Referrer-Policy", "strict-origin-when-cross-origin")]
    [InlineData("Cross-Origin-Opener-Policy", "same-origin")]
    public async Task Sign_in_page_sends_the_security_header(string header, string expected)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/account/login", UriKind.Relative));

        Assert.Equal(expected, Assert.Single(response.Headers.GetValues(header)));
    }

    [Fact]
    public async Task Content_security_policy_forbids_inline_and_third_party_code()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/account/login", UriKind.Relative));
        var policy = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));

        Assert.Contains("default-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("script-src 'self'", policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe", policy, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Error_pages_keep_the_security_headers()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsync();

        using var response = await client.GetAsync(new Uri("/no/such/page", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task Pages_with_employee_data_are_never_cached()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsync();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.True(response.Headers.CacheControl?.NoStore);
    }
}
