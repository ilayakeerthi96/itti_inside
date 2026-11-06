using System.Net;

namespace Itti.Inside.Tests;

public sealed class PortalPagesTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Theory]
    [InlineData("Applications")]
    [InlineData("QA")]
    [InlineData("HR")]
    [InlineData("Quality Management System")]
    [InlineData("Process Asset Library")]
    [InlineData("Corporate Medical Insurance")]
    [InlineData("HOLIDAYS")]
    [InlineData("Organization Training")]
    [InlineData("Useful Links")]
    [InlineData("Travel desk")]
    [InlineData("Logout")]
    public async Task Header_has_every_menu_item(string menuItem)
    {
        Assert.Contains(menuItem, await HeaderAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Applications", "https://inside.itti.com/application-wp.asp")]
    [InlineData("Quality Management System", "https://inside.itti.com/qms_new/index.asp")]
    [InlineData("Process Asset Library", "https://inside.itti.com/PALibrary/default.asp")]
    [InlineData("Logout", "https://inside.itti.com/Logout.asp")]
    public async Task Menu_links_go_to_the_current_intranet(string menuItem, string address)
    {
        Assert.Contains($"href=\"{address}\">{menuItem}</a>", await HeaderAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Home", "/")]
    [InlineData("Travel desk", "/hr/travel-desk")]
    public async Task Menu_links_to_this_site_stay_on_it(string menuItem, string path)
    {
        Assert.Contains($"href=\"{path}\">{menuItem}</a>", await HeaderAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Landing_page_opens_straight_away_without_a_sign_in_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Welcome to ITTI Inside", html, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"password\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Landing_page_has_no_master_link()
    {
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        Assert.DoesNotContain("/master", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("CEO&#x27;s Desk")]
    [InlineData("Customer Feedback")]
    [InlineData("Suggestion Box")]
    public async Task Header_leaves_out_the_removed_items(string menuItem)
    {
        Assert.DoesNotContain(menuItem, await HeaderAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Header_dropdowns_list_the_items_in_order()
    {
        var header = await HeaderAsync();

        Assert.True(
            header.IndexOf("Quality Management System", StringComparison.Ordinal)
                < header.IndexOf("Process Asset Library", StringComparison.Ordinal));
        Assert.True(
            header.IndexOf("Corporate Medical Insurance", StringComparison.Ordinal)
                < header.IndexOf("Travel desk", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Latest News")]
    [InlineData("CEO's Desk")]
    [InlineData("Customer Feedback")]
    [InlineData("Birthdays")]
    [InlineData("Rockstar Awards")]
    [InlineData("Wedding Bells")]
    [InlineData("New Born")]
    public async Task Landing_page_keeps_every_existing_section(string section)
    {
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        Assert.Contains(section, html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/applications")]
    [InlineData("/applications/timesheet")]
    [InlineData("/qa/quality-management-system")]
    [InlineData("/qa/process-asset-library")]
    [InlineData("/hr/corporate-medical-insurance")]
    [InlineData("/hr/holidays-2026")]
    [InlineData("/hr/organization-training")]
    [InlineData("/hr/useful-links")]
    [InlineData("/hr/travel-desk")]
    [InlineData("/ceo-desk")]
    [InlineData("/customer-feedback")]
    [InlineData("/rockstar-awards")]
    public async Task Portal_pages_open_without_signing_in(string path)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_page_shows_the_not_found_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/no/such/page", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("September 2026")]
    [InlineData("July 2026")]
    [InlineData("October 2025")]
    public async Task Every_award_is_on_the_page_for_the_slider(string award)
    {
        Assert.Contains(award, await MainPageAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("customer-feedback", "2")]
    [InlineData("birthdays", "5")]
    [InlineData("rockstar-awards", "2")]
    [InlineData("wedding-bells", "2")]
    [InlineData("new-born", "2")]
    public async Task Each_list_card_pages_through_its_items(string card, string pageSize)
    {
        var html = await MainPageAsync();
        var start = html.IndexOf($"id=\"{card}\"", StringComparison.Ordinal);
        var section = html[start..html.IndexOf("</section>", start, StringComparison.Ordinal)];

        Assert.Contains($"data-page-size=\"{pageSize}\"", section, StringComparison.Ordinal);
        Assert.Contains("data-pager-controls hidden", section, StringComparison.Ordinal);
        Assert.Contains("data-pager-item", section, StringComparison.Ordinal);
    }

    private async Task<string> MainPageAsync()
    {
        using var client = factory.CreateBrowser();

        return await client.GetStringAsync(new Uri("/", UriKind.Relative));
    }

    [Fact]
    public async Task Health_endpoint_needs_no_sign_in()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Only the site header, so the section titles further down the page don't count.
    private async Task<string> HeaderAsync()
    {
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));
        var start = html.IndexOf("<header class=\"site-header\">", StringComparison.Ordinal);
        var end = html.IndexOf("</header>", start, StringComparison.Ordinal);

        return html[start..end];
    }
}
