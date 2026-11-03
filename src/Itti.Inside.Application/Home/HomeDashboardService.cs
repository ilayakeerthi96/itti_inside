using Itti.Inside.Application.Abstractions;

namespace Itti.Inside.Application.Home;

public interface IHomeDashboardService
{
    Task<HomeDashboard> GetAsync(CancellationToken cancellationToken);
}

public sealed class HomeDashboardService(IPortalContentRepository content) : IHomeDashboardService
{
    // The landing page pages through these lists in the browser, so it loads only the newest ones.
    // The full lists belong on the Customer Feedback and Rockstar Awards pages.
    private const int FeedbackShown = 10;
    private const int AwardsShown = 12;

    // Awaited one at a time on purpose: an EF Core DbContext does not allow concurrent queries.
    public async Task<HomeDashboard> GetAsync(CancellationToken cancellationToken)
    {
        var heroSlides = await content.GetHeroSlidesAsync(cancellationToken);
        var latestNews = await content.GetLatestNewsAsync(cancellationToken);
        var ceoMessage = await content.GetCeoMessageAsync(cancellationToken);
        var customerFeedback = await content.GetCustomerFeedbackAsync(1, FeedbackShown, cancellationToken);
        var birthdays = await content.GetBirthdaysAsync(cancellationToken);
        var rockstarAwards = await content.GetRockstarAwardsAsync(1, AwardsShown, cancellationToken);
        var weddingBells = await content.GetWeddingBellsAsync(cancellationToken);
        var newBorns = await content.GetNewBornsAsync(cancellationToken);

        return new HomeDashboard(
            heroSlides,
            latestNews,
            ceoMessage,
            customerFeedback,
            birthdays,
            rockstarAwards,
            weddingBells,
            newBorns);
    }
}
