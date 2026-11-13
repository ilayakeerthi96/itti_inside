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
    [InlineData("Logout")]
    public async Task Header_has_every_menu_item(string menuItem)
    {
        Assert.Contains(menuItem, await HeaderAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Applications", "https://inside.itti.com/application-wp.asp", true)]
    [InlineData("Quality Management System", "https://inside.itti.com/qms_new/index.asp", true)]
    [InlineData("Process Asset Library", "https://inside.itti.com/PALibrary/default.asp", true)]
    [InlineData("Organization Training", "https://training.ittiapps.cloud/#/login", true)]
    [InlineData("Home", "/", false)]
    [InlineData("Corporate Medical Insurance", "/hr/corporate-medical-insurance", false)]
    [InlineData("HOLIDAYS – 2026", "/hr/holidays", false)]
    [InlineData("Logout", "https://inside.itti.com/Logout.asp", false)]
    public async Task Menu_links_go_to_their_page(string menuItem, string address, bool newTab)
    {
        var link = PageLinks.Find(await HeaderAsync(), menuItem);

        Assert.Equal(address, link.Href);
        Assert.Equal(newTab, link.OpensInNewTab);
    }

    [Theory]
    [InlineData("More", "/ceo-desk")]
    [InlineData("View all feedback", "/customer-feedback")]
    [InlineData("View all birthdays", "/birthdays")]
    public async Task Landing_page_links_open_in_the_same_tab(string text, string address)
    {
        var link = PageLinks.Find(await MainContentAsync(), text);

        Assert.Equal(address, link.Href);
        Assert.False(link.OpensInNewTab, $"{text} should open in the same tab.");
    }

    [Fact]
    public async Task Read_more_opens_the_awards_page_at_that_award()
    {
        var link = PageLinks.Find(await MainContentAsync(), "Read More");

        Assert.Matches("^/rockstar-awards#award-\\d+$", link.Href);
        Assert.False(link.OpensInNewTab);
    }

    [Theory]
    [InlineData("/wedding-bells")]
    [InlineData("/new-born")]
    public async Task Celebration_cards_link_to_their_full_page(string address)
    {
        Assert.Contains(
            PageLinks.All(await MainContentAsync()),
            link => link.Text == "View all" && link.Href == address && !link.OpensInNewTab);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/customer-feedback")]
    [InlineData("/rockstar-awards")]
    [InlineData("/birthdays")]
    [InlineData("/wedding-bells")]
    [InlineData("/new-born")]
    [InlineData("/ceo-desk")]
    [InlineData("/hr/corporate-medical-insurance")]
    [InlineData("/hr/holidays")]
    [InlineData("/hr/organization-training")]
    [InlineData("/qa/quality-management-system")]
    [InlineData("/applications")]
    public async Task Only_applications_qa_and_training_open_in_a_new_tab(string path)
    {
        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        var newTab = PageLinks.All(html).Where(link => link.OpensInNewTab).Select(link => link.Text).Order().ToList();

        Assert.Equal(["Applications", "Organization Training", "Process Asset Library", "Quality Management System"], newTab);
    }

    private async Task<string> MainContentAsync()
    {
        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        return html[html.IndexOf("<main", StringComparison.Ordinal)..];
    }

    [Fact]
    public async Task Screen_readers_hear_that_a_link_opens_a_new_tab()
    {
        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        Assert.Contains("<span id=\"new-tab-note\" hidden>Opens in a new tab</span>", html, StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"new-tab-note\"", PageLinks.Find(html, "Applications").Tag, StringComparison.Ordinal);
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
                < header.IndexOf("Organization Training", StringComparison.Ordinal));
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
    [InlineData("/hr/holidays")]
    [InlineData("/hr/organization-training")]
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
