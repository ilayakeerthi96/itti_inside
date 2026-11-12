using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Home;

public interface IHomeDashboardService
{
    Task<HomeDashboard> GetAsync(CancellationToken cancellationToken);
}

public sealed class HomeDashboardService(IPortalContentRepository content, TimeProvider clock) : IHomeDashboardService
{
    // The landing page pages through these lists in the browser, so it loads only the newest ones.
    // The full lists are on the Customer Feedback and Rockstar Awards pages.
    private const int FeedbackShown = 10;
    private const int AwardsShown = 12;

    // Awaited one at a time on purpose: an EF Core DbContext does not allow concurrent queries.
    public async Task<HomeDashboard> GetAsync(CancellationToken cancellationToken)
    {
        var heroSlides = await content.GetHeroSlidesAsync(cancellationToken);
        var latestNews = await content.GetLatestNewsAsync(cancellationToken);
        var ceoPosts = await content.GetCeoDeskPostsAsync(cancellationToken);
        var ceoDesk = ceoPosts.Count > 0 ? ceoPosts[0] : null;
        var customerFeedback = await content.GetCustomerFeedbackAsync(1, FeedbackShown, cancellationToken);
        var birthdays = await content.GetBirthdaysAsync(cancellationToken);
        var rockstarAwards = await content.GetRockstarAwardsAsync(1, AwardsShown, cancellationToken);
        var weddingBells = await content.GetWeddingBellsAsync(cancellationToken);
        var newBorns = await content.GetNewBornsAsync(cancellationToken);

        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

        return new HomeDashboard(
            heroSlides,
            latestNews,
            ceoDesk,
            customerFeedback,
            RecentBirthdays(birthdays, today),
            rockstarAwards,
            RecentAnnouncements(weddingBells, today),
            RecentAnnouncements(newBorns, today));
    }

    // This month's birthdays run by day of the month. Earlier ones keep the newest posted first.
    private static CardContent<BirthdayCalendar> RecentBirthdays(BirthdayCalendar calendar, DateOnly today)
    {
        var recent = RecentUpdates.Select(calendar.Entries, entry => entry.PostedOn, today);
        var entries = recent.ShowsEarlierUpdates
            ? recent.Content
            : recent.Content.OrderBy(entry => entry.Day).ThenBy(entry => entry.Id).ToList();

        return new CardContent<BirthdayCalendar>(calendar with { Entries = entries }, recent.ShowsEarlierUpdates);
    }

    private static CardContent<CelebrationBoard> RecentAnnouncements(CelebrationBoard board, DateOnly today)
    {
        var recent = RecentUpdates.Select(board.Announcements, announcement => announcement.PostedOn, today);

        return new CardContent<CelebrationBoard>(board with { Announcements = recent.Content }, recent.ShowsEarlierUpdates);
    }
}
