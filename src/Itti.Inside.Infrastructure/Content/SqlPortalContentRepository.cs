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

    public async Task<IReadOnlyList<CeoDeskPost>> GetCeoDeskPostsAsync(CancellationToken cancellationToken) =>
        await context.CeoDeskPosts
            .OrderByDescending(post => post.PostedOn)
            .ThenByDescending(post => post.Id)
            .ToListAsync(cancellationToken);

    public Task<PagedResult<CustomerFeedbackEntry>> GetCustomerFeedbackAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        context.CustomerFeedback
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToPageAsync(page, pageSize, cancellationToken);

    public async Task<IReadOnlyList<CustomerFeedbackEntry>> GetAllCustomerFeedbackAsync(CancellationToken cancellationToken) =>
        await context.CustomerFeedback
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);

    public async Task<BirthdayCalendar> GetBirthdaysAsync(CancellationToken cancellationToken)
    {
        var period = await GetTextAsync(PortalTextKey.BirthdaysPeriod, cancellationToken);
        var entries = await context.Birthdays
            .OrderByDescending(entry => entry.PostedOn)
            .ThenBy(entry => entry.Day)
            .ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);

        return new BirthdayCalendar(period, entries);
    }

    public async Task<PagedResult<RockstarAward>> GetRockstarAwardsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var awards = await context.RockstarAwards
            .OrderByDescending(award => award.PublishedOn)
            .ThenByDescending(award => award.Id)
            .ToPageAsync(page, pageSize, cancellationToken);

        return awards with { Items = await WithImagesAsync(awards.Items, cancellationToken) };
    }

    public async Task<IReadOnlyList<RockstarAward>> GetAllRockstarAwardsAsync(CancellationToken cancellationToken) =>
        await WithImagesAsync(
            await context.RockstarAwards
                .OrderByDescending(award => award.PublishedOn)
                .ThenByDescending(award => award.Id)
                .ToListAsync(cancellationToken),
            cancellationToken);

    // One query for the pictures of all the awards on show, rather than one per award.
    private async Task<IReadOnlyList<RockstarAward>> WithImagesAsync(IReadOnlyList<RockstarAward> awards, CancellationToken cancellationToken)
    {
        var awardIds = awards.Select(award => award.Id).ToList();
        var links = await context.RockstarAwardImages
            .Where(link => awardIds.Contains(link.AwardId))
            .OrderBy(link => link.Position)
            .ThenBy(link => link.Id)
            .ToListAsync(cancellationToken);
        var imagesByAward = links.ToLookup(link => link.AwardId, link => link.ImageId);

        return awards.Select(award => award with { ImageIds = imagesByAward[award.Id].ToList() }).ToList();
    }

    public async Task<CelebrationBoard> GetWeddingBellsAsync(CancellationToken cancellationToken)
    {
        var closing = await GetTextAsync(PortalTextKey.WeddingBellsClosing, cancellationToken);
        var announcements = await context.WeddingAnnouncements
            .OrderByDescending(item => item.PostedOn)
            .ThenByDescending(item => item.Id)
            .Select(item => new CelebrationAnnouncement(item.Id, item.Text, item.PostedOn))
            .ToListAsync(cancellationToken);

        return new CelebrationBoard(announcements, closing);
    }

    public async Task<CelebrationBoard> GetNewBornsAsync(CancellationToken cancellationToken)
    {
        var closing = await GetTextAsync(PortalTextKey.NewBornClosing, cancellationToken);
        var announcements = await context.NewBornAnnouncements
            .OrderByDescending(item => item.PostedOn)
            .ThenByDescending(item => item.Id)
            .Select(item => new CelebrationAnnouncement(item.Id, item.Text, item.PostedOn))
            .ToListAsync(cancellationToken);

        return new CelebrationBoard(announcements, closing);
    }

    public async Task<IReadOnlyList<Holiday>> GetHolidaysAsync(CancellationToken cancellationToken) =>
        await context.Holidays
            .OrderBy(holiday => holiday.Date)
            .ThenBy(holiday => holiday.Id)
            .ToListAsync(cancellationToken);

    public Task<string> GetHolidayNotesAsync(CancellationToken cancellationToken) =>
        GetTextAsync(PortalTextKey.HolidayNotes, cancellationToken);

    public async Task<IReadOnlyList<TrainingSession>> GetTrainingSessionsAsync(CancellationToken cancellationToken) =>
        await context.TrainingSessions
            .OrderBy(session => session.Date)
            .ThenBy(session => session.Id)
            .ToListAsync(cancellationToken);

    public Task<MedicalInsurancePolicy?> GetMedicalInsuranceAsync(CancellationToken cancellationToken) =>
        context.MedicalInsurancePolicies.OrderBy(policy => policy.Id).FirstOrDefaultAsync(cancellationToken);

    public Task<PortalDocumentInfo?> GetDocumentInfoAsync(string key, CancellationToken cancellationToken) =>
        context.PortalDocuments
            .Where(document => document.Key == key)
            .Select(document => new PortalDocumentInfo(document.Key, document.FileName, document.Content.Length, document.UploadedOn))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<PortalDocument?> GetDocumentAsync(string key, CancellationToken cancellationToken) =>
        context.PortalDocuments.FirstOrDefaultAsync(document => document.Key == key, cancellationToken);

    public Task<PortalImage?> GetImageAsync(int id, CancellationToken cancellationToken) =>
        context.PortalImages.FirstOrDefaultAsync(image => image.Id == id, cancellationToken);

    private async Task<string> GetTextAsync(PortalTextKey key, CancellationToken cancellationToken) =>
        await context.PortalTexts
            .Where(text => text.Key == key)
            .Select(text => text.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
}
