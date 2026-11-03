using Itti.Inside.Application.Abstractions;
using Itti.Inside.Application.Common;
using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Itti.Inside.Infrastructure.Content;

/// <summary>Serves landing page content from the database, so master page saves show at once.</summary>
public sealed class SqlPortalContentRepository(PortalDbContext context) : IPortalContentRepository
{
    public async Task<IReadOnlyList<HeroSlide>> GetHeroSlidesAsync(CancellationToken cancellationToken) =>
        await context.HeroSlides.OrderBy(slide => slide.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NewsItem>> GetLatestNewsAsync(CancellationToken cancellationToken) =>
        await context.NewsItems.OrderByDescending(item => item.Id).ToListAsync(cancellationToken);

    public async Task<CeoMessage> GetCeoMessageAsync(CancellationToken cancellationToken) =>
        new(await GetTextAsync(PortalTextKey.CeoMessage, cancellationToken));

    public Task<PagedResult<CustomerFeedbackEntry>> GetCustomerFeedbackAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        context.CustomerFeedback
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToPageAsync(page, pageSize, cancellationToken);

    public async Task<BirthdayCalendar> GetBirthdaysAsync(CancellationToken cancellationToken)
    {
        var period = await GetTextAsync(PortalTextKey.BirthdaysPeriod, cancellationToken);
        var entries = await context.Birthdays
            .OrderBy(entry => entry.Day)
            .ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);

        return new BirthdayCalendar(period, entries);
    }

    public Task<PagedResult<RockstarAward>> GetRockstarAwardsAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        context.RockstarAwards
            .OrderByDescending(award => award.PublishedOn)
            .ThenByDescending(award => award.Id)
            .ToPageAsync(page, pageSize, cancellationToken);

    public async Task<CelebrationBoard> GetWeddingBellsAsync(CancellationToken cancellationToken)
    {
        var closing = await GetTextAsync(PortalTextKey.WeddingBellsClosing, cancellationToken);
        var announcements = await context.WeddingAnnouncements
            .OrderByDescending(item => item.Id)
            .Select(item => item.Text)
            .ToListAsync(cancellationToken);

        return new CelebrationBoard(announcements, closing);
    }

    public async Task<CelebrationBoard> GetNewBornsAsync(CancellationToken cancellationToken)
    {
        var closing = await GetTextAsync(PortalTextKey.NewBornClosing, cancellationToken);
        var announcements = await context.NewBornAnnouncements
            .OrderByDescending(item => item.Id)
            .Select(item => item.Text)
            .ToListAsync(cancellationToken);

        return new CelebrationBoard(announcements, closing);
    }

    private async Task<string> GetTextAsync(PortalTextKey key, CancellationToken cancellationToken) =>
        await context.PortalTexts
            .Where(text => text.Key == key)
            .Select(text => text.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
}
