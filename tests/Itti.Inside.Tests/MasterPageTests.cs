using System.Net;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itti.Inside.Tests;

public sealed class MasterSectionTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Theory]
    [InlineData("/master/hero-slides", "Transforming Technology Together")]
    [InlineData("/master/latest-news", "Dear ITTIians")]
    [InlineData("/master/ceo-desk", "Dear Ittians")]
    [InlineData("/master/customer-feedback", "Unilever")]
    [InlineData("/master/birthdays", "Satyajit Das")]
    [InlineData("/master/rockstar-awards", "Robin Nath")]
    [InlineData("/master/wedding-bells", "Suhas S")]
    [InlineData("/master/new-born", "Dharanidharan")]
    public async Task Every_section_opens_with_the_current_content(string path, string expected)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        Assert.Contains(expected, html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/master/customer-feedback")]
    [InlineData("/master/rockstar-awards")]
    public async Task Date_fields_use_the_date_picker_and_an_unambiguous_format(string path)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        Assert.Contains("type=\"date\"", html, StringComparison.Ordinal);
        Assert.Matches("type=\"date\"[^>]*value=\"\\d{4}-\\d{2}-\\d{2}\"", html);
    }

    [Fact]
    public async Task Master_address_opens_the_first_section()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.GetAsync(new Uri("/master", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/master/hero-slides", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_header_has_only_master_and_logout()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await client.GetStringAsync(new Uri("/master/hero-slides", UriKind.Relative));
        var header = html[..html.IndexOf("</header>", StringComparison.Ordinal)];

        Assert.Contains("href=\"/master\"", header, StringComparison.Ordinal);
        Assert.Contains("Logout", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Applications", header, StringComparison.Ordinal);
        Assert.DoesNotContain("dropdown-toggle", header, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/ceo-desk")]
    [InlineData("/hr/holidays")]
    public async Task Admin_sees_the_portal_pages_as_everyone_does(string path)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));
        var header = html[..html.IndexOf("</header>", StringComparison.Ordinal)];

        Assert.Contains("Applications", header, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/master\"", header, StringComparison.Ordinal);
    }
}

public sealed class MasterUpdateTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task Changing_the_ceo_message_updates_the_main_page()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            "/master/ceo-desk",
            "/master/ceo-desk",
            new("Form.Title", "A new title from the CEO"),
            new("Form.PostedOn", DateTime.Today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)),
            new("Form.Message", "A new message from the CEO."));

        var html = await MainPageAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("A new title from the CEO", html, StringComparison.Ordinal);
        Assert.Contains("A new message from the CEO.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adding_customer_feedback_updates_the_main_page()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            "/master/customer-feedback",
            "/master/customer-feedback",
            new("Form.Date", "2030-01-15"),
            new("Form.Client", "Brand New Client"),
            new("Form.Team", "Brand New Team"),
            new("Form.Excerpt", "Thank you for the great work."));

        var html = await MainPageAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Brand New Client", html, StringComparison.Ordinal);
        Assert.Contains("Thank you for the great work.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adding_a_birthday_updates_the_main_page()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            "/master/birthdays",
            "/master/birthdays",
            new("Form.Name", "Birthday Person"),
            new("Form.EmployeeId", "9999"),
            new("Form.Day", "21"),
            new("Form.Email", "birthday.person@itti.com"),
            new("Form.Location", "Bangalore"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Birthday Person", await MainPageAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Editing_an_award_updates_the_main_page()
    {
        var award = await SaveAsync(new RockstarAward(0, "Award before editing", new DateOnly(2030, 3, 1), "Text."));

        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            $"/master/rockstar-awards?edit={award.Id}",
            "/master/rockstar-awards",
            new("Form.Id", award.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("Form.Title", "Award after editing"),
            new("Form.PublishedOn", "2030-03-02"),
            new("Form.Excerpt", "Text."));

        var html = await MainPageAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Award after editing", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Award before editing", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Deleting_news_removes_it_from_the_main_page()
    {
        var news = await SaveAsync(new NewsItem(0, "News that will be deleted."));

        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        Assert.Contains("News that will be deleted.", await MainPageAsync(), StringComparison.Ordinal);

        using var response = await client.PostFormAsync("/master/latest-news", $"/master/latest-news/{news.Id}/delete");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("News that will be deleted.", await MainPageAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/master/wedding-bells", "A wedding announcement.")]
    [InlineData("/master/new-born", "A new born announcement.")]
    public async Task Adding_an_announcement_puts_it_on_the_main_page(string section, string text)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(section, section, KeyValuePair.Create("Form.Text", text));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(text, await MainPageAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_form_saves_nothing_and_shows_the_problem()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            "/master/birthdays",
            "/master/birthdays",
            new("Form.Name", "Not Saved Person"),
            new("Form.EmployeeId", "9998"),
            new("Form.Day", "45"),
            new("Form.Email", "not-an-email"),
            new("Form.Location", "Bangalore"));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The Email field is not a valid e-mail address.", html, StringComparison.Ordinal);
        Assert.Contains("must be between 1 and 31", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Not Saved Person", await MainPageAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saving_an_item_that_was_deleted_adds_nothing()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.PostFormAsync(
            "/master/latest-news",
            "/master/latest-news",
            new("Form.Id", "999999"),
            new("Form.Text", "Ghost news."));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("Ghost news.", await MainPageAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Opening_an_item_that_does_not_exist_shows_not_found()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.GetAsync(new Uri("/master/latest-news?edit=999999", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Html_typed_into_a_form_is_shown_as_plain_text()
    {
        await SaveAsync(new NewsItem(0, "<script>alert('typed in')</script>"));

        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await MainPageAsync();

        Assert.DoesNotContain("<script>alert", html, StringComparison.Ordinal);
        Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Posting_without_an_anti_forgery_token_is_rejected()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["Form.Text"] = "Forged news." });
        using var response = await client.PostAsync(new Uri("/master/latest-news", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("Forged news.", await MainPageAsync(), StringComparison.Ordinal);
    }

    // The main page as everyone sees it, without signing in.
    private async Task<string> MainPageAsync()
    {
        using var visitor = factory.CreateBrowser();

        return await visitor.GetStringAsync(new Uri("/", UriKind.Relative));
    }

    private async Task<T> SaveAsync<T>(T item)
        where T : class, IContentItem
    {
        using var scope = factory.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IPortalContentEditor>().SaveAsync(item, CancellationToken.None);

        return item;
    }
}

public sealed class EmptyContentTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task Main_page_still_opens_when_every_list_is_empty()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();

            await context.HeroSlides.ExecuteDeleteAsync();
            await context.NewsItems.ExecuteDeleteAsync();
            await context.CustomerFeedback.ExecuteDeleteAsync();
            await context.Birthdays.ExecuteDeleteAsync();
            await context.RockstarAwards.ExecuteDeleteAsync();
            await context.WeddingAnnouncements.ExecuteDeleteAsync();
            await context.NewBornAnnouncements.ExecuteDeleteAsync();
            await context.CeoDeskPosts.ExecuteDeleteAsync();
        }

        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class MasterAccessTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Theory]
    [InlineData("/master")]
    [InlineData("/master/ceo-desk")]
    [InlineData("/master/birthdays?edit=1")]
    public async Task Employee_cannot_open_the_master_page(string path)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsync();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/error/403", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/master")]
    [InlineData("/master/customer-feedback")]
    [InlineData("/master/birthdays?edit=1")]
    public async Task Signed_out_visitor_is_sent_to_the_sign_in_page(string path)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Signed_out_visitor_cannot_save_changes()
    {
        using var client = factory.CreateBrowser();
        var token = await client.GetAntiforgeryTokenAsync("/account/login");

        using var response = await client.PostAsync(new Uri("/master/text", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Key"] = nameof(PortalTextKey.BirthdaysPeriod),
            ["Value"] = "Visitor message.",
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Visitor message.",
            await client.GetStringAsync(new Uri("/", UriKind.Relative)),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Employee_cannot_save_changes()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsync();

        // The admin check comes before the anti-forgery check, so no token is needed to show the refusal.
        using var response = await client.PostAsync(new Uri("/master/text", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Key"] = nameof(PortalTextKey.BirthdaysPeriod),
            ["Value"] = "Employee message.",
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/error/403", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Employee message.",
            await client.GetStringAsync(new Uri("/", UriKind.Relative)),
            StringComparison.Ordinal);
    }
}

public sealed class SingleSignInTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task There_is_no_separate_admin_sign_in_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/admin/login", UriKind.Relative));

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_account_goes_to_the_master_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.AdminId, PortalWebFactory.AdminPassword);
        using var master = await client.GetAsync(new Uri("/master/ceo-desk", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/master", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, master.StatusCode);
    }

    [Fact]
    public async Task Employee_account_goes_to_the_main_page()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Admin_returns_to_the_master_section_they_asked_for()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.AdminId, PortalWebFactory.AdminPassword, "/master/birthdays");

        Assert.Equal("/master/birthdays", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/hr/travel-desk")]
    public async Task Admin_asked_for_a_portal_page_still_goes_to_the_master_page(string returnUrl)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.PostSignInAsync(PortalWebFactory.AdminId, PortalWebFactory.AdminPassword, returnUrl);

        Assert.Equal("/master", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Signed_in_admin_opening_the_sign_in_page_goes_to_the_master_page()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var response = await client.GetAsync(new Uri("/account/login", UriKind.Relative));

        Assert.Equal("/master", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Visitor_sees_admin_updates_without_signing_in()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        using var saved = await admin.PostFormAsync(
            "/master/latest-news",
            "/master/latest-news",
            KeyValuePair.Create("Form.Text", "News from the admin."));

        using var visitor = factory.CreateBrowser();
        var home = await visitor.GetStringAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.Contains("News from the admin.", home, StringComparison.Ordinal);
        Assert.Contains("Applications", home, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/master\"", home, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Admin_logout_returns_to_the_sign_in_page()
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        using var logout = await client.PostFormAsync("/master/ceo-desk", "/account/logout");

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/account/login", logout.Headers.Location?.OriginalString);
    }
}
