using System.Net;
using System.Text.RegularExpressions;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Application.Home;
using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itti.Inside.Tests;

/// <summary>The full pages behind the landing page cards.</summary>
public sealed partial class DetailPagesTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Theory]
    [InlineData("/ceo-desk", "An update from the CEO", "Dear Ittians")]
    [InlineData("/customer-feedback", "All customer feedback", "Unilever")]
    [InlineData("/rockstar-awards", "All Rockstar Awards", "Robin Nath")]
    [InlineData("/birthdays", "Birthdays", "Satyajit Das")]
    [InlineData("/wedding-bells", "All Wedding Bells announcements", "Suhas S")]
    [InlineData("/new-born", "All New Born announcements", "Dharanidharan")]
    public async Task Each_page_opens_without_signing_in_and_shows_its_content(string path, string heading, string content)
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(heading, html, StringComparison.Ordinal);
        Assert.Contains(content, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Customer_feedback_page_shows_every_entry_not_just_the_cards_ten()
    {
        int stored;
        using (var scope = factory.Services.CreateScope())
        {
            stored = await scope.ServiceProvider.GetRequiredService<PortalDbContext>().CustomerFeedback.CountAsync();
        }

        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/customer-feedback", UriKind.Relative));

        Assert.Equal(stored, FeedbackItem().Count(html));
    }

    [Fact]
    public async Task Every_award_has_an_anchor_for_read_more()
    {
        using var client = factory.CreateBrowser();
        var home = await client.GetStringAsync(new Uri("/", UriKind.Relative));
        var awards = await client.GetStringAsync(new Uri("/rockstar-awards", UriKind.Relative));

        var target = PageLinks.Find(home[home.IndexOf("<main", StringComparison.Ordinal)..], "Read More").Href.Split('#')[1];

        Assert.Contains($"id=\"{target}\"", awards, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/master/birthdays")]
    [InlineData("/master/wedding-bells")]
    [InlineData("/master/new-born")]
    public async Task Master_forms_ask_for_the_posted_on_date(string path)
    {
        using var client = factory.CreateBrowser();
        await client.SignInAsAdminAsync();

        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        Assert.Matches("type=\"date\"[^>]*name=\"Form.PostedOn\"|name=\"Form.PostedOn\"[^>]*type=\"date\"", html);
        Assert.Contains("Posted on", html, StringComparison.Ordinal);
    }

    [GeneratedRegex("<li class=\"feedback\" id=\"feedback-\\d+\"")]
    private static partial Regex FeedbackItem();
}

/// <summary>Cards with nothing new this month show the last two updates. Own database, because it empties lists.</summary>
public sealed class EarlierUpdatesTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task Wedding_bells_without_news_this_month_show_the_last_two()
    {
        await ClearAsync(context => context.WeddingAnnouncements.ExecuteDeleteAsync());
        await SaveAsync(new WeddingAnnouncement(0, "Oldest wedding.", Today.AddMonths(-3)));
        await SaveAsync(new WeddingAnnouncement(0, "Older wedding.", Today.AddMonths(-2)));
        await SaveAsync(new WeddingAnnouncement(0, "Last month's wedding.", Today.AddMonths(-1)));

        var card = await CardAsync("wedding-bells");
        var page = await PageAsync("/wedding-bells");

        Assert.Contains("Last month&#x27;s wedding.", card, StringComparison.Ordinal);
        Assert.Contains("Older wedding.", card, StringComparison.Ordinal);
        Assert.DoesNotContain("Oldest wedding.", card, StringComparison.Ordinal);
        Assert.Contains("Nothing new this month", card, StringComparison.Ordinal);
        Assert.Contains("Oldest wedding.", page, StringComparison.Ordinal);

        await SaveAsync(new WeddingAnnouncement(0, "This month's wedding.", Today));
        var updated = await CardAsync("wedding-bells");

        Assert.Contains("This month&#x27;s wedding.", updated, StringComparison.Ordinal);
        Assert.DoesNotContain("Last month&#x27;s wedding.", updated, StringComparison.Ordinal);
        Assert.DoesNotContain("Nothing new this month", updated, StringComparison.Ordinal);
    }

    [Fact]
    public async Task New_born_without_news_this_month_shows_the_last_two()
    {
        await ClearAsync(context => context.NewBornAnnouncements.ExecuteDeleteAsync());
        await SaveAsync(new NewBornAnnouncement(0, "Oldest baby.", Today.AddMonths(-5)));
        await SaveAsync(new NewBornAnnouncement(0, "Older baby.", Today.AddMonths(-4)));
        await SaveAsync(new NewBornAnnouncement(0, "Recent baby.", Today.AddMonths(-2)));

        var card = await CardAsync("new-born");

        Assert.Contains("Recent baby.", card, StringComparison.Ordinal);
        Assert.Contains("Older baby.", card, StringComparison.Ordinal);
        Assert.DoesNotContain("Oldest baby.", card, StringComparison.Ordinal);
        Assert.Contains("Nothing new this month", card, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Birthdays_without_news_this_month_show_the_last_two()
    {
        await ClearAsync(context => context.Birthdays.ExecuteDeleteAsync());
        await SaveAsync(new BirthdayEntry(0, "9001", "Oldest Person", 5, "oldest@itti.com", "Bangalore", Today.AddMonths(-3)));
        await SaveAsync(new BirthdayEntry(0, "9002", "Older Person", 9, "older@itti.com", "Chennai", Today.AddMonths(-2)));
        await SaveAsync(new BirthdayEntry(0, "9003", "Recent Person", 14, "recent@itti.com", "Pune", Today.AddMonths(-1)));

        var card = await CardAsync("birthdays");
        var page = await PageAsync("/birthdays");

        Assert.Contains("Recent Person", card, StringComparison.Ordinal);
        Assert.Contains("Older Person", card, StringComparison.Ordinal);
        Assert.DoesNotContain("Oldest Person", card, StringComparison.Ordinal);
        Assert.Contains("No new birthdays this month", card, StringComparison.Ordinal);
        Assert.Contains("Oldest Person", page, StringComparison.Ordinal);
    }

    private async Task<string> CardAsync(string id)
    {
        var html = await PageAsync("/");
        var start = html.IndexOf($"id=\"{id}\"", StringComparison.Ordinal);

        return html[start..html.IndexOf("</section>", start, StringComparison.Ordinal)];
    }

    private async Task<string> PageAsync(string path)
    {
        using var client = factory.CreateBrowser();

        return await client.GetStringAsync(new Uri(path, UriKind.Relative));
    }

    private async Task ClearAsync(Func<PortalDbContext, Task<int>> clear)
    {
        using var scope = factory.Services.CreateScope();

        await clear(scope.ServiceProvider.GetRequiredService<PortalDbContext>());
    }

    private async Task SaveAsync<T>(T item)
        where T : class, IContentItem
    {
        using var scope = factory.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IPortalContentEditor>().SaveAsync(item, CancellationToken.None);
    }
}

public sealed class RecentUpdatesTests
{
    private static readonly DateOnly Today = new(2026, 11, 12);

    [Fact]
    public void Shows_everything_posted_this_month()
    {
        var recent = RecentUpdates.Select([new DateOnly(2026, 11, 30), new DateOnly(2026, 11, 1), new DateOnly(2026, 10, 31)], day => day, Today);

        Assert.Equal([new DateOnly(2026, 11, 30), new DateOnly(2026, 11, 1)], recent.Content);
        Assert.False(recent.ShowsEarlierUpdates);
    }

    [Fact]
    public void Shows_the_last_two_when_nothing_is_new_this_month()
    {
        var recent = RecentUpdates.Select([new DateOnly(2026, 10, 3), new DateOnly(2026, 9, 9), new DateOnly(2026, 8, 1)], day => day, Today);

        Assert.Equal([new DateOnly(2026, 10, 3), new DateOnly(2026, 9, 9)], recent.Content);
        Assert.True(recent.ShowsEarlierUpdates);
    }

    [Fact]
    public void The_same_month_last_year_is_not_new()
    {
        var recent = RecentUpdates.Select([new DateOnly(2025, 11, 20)], day => day, Today);

        Assert.True(recent.ShowsEarlierUpdates);
    }

    [Fact]
    public void An_empty_list_stays_empty()
    {
        var recent = RecentUpdates.Select(Array.Empty<DateOnly>(), day => day, Today);

        Assert.Empty(recent.Content);
        Assert.False(recent.ShowsEarlierUpdates);
    }
}
