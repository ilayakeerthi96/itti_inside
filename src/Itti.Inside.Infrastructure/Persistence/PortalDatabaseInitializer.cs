using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Itti.Inside.Infrastructure.Persistence;

/// <summary>Brings the database up to date and gives a new database its first content.</summary>
public sealed class PortalDatabaseInitializer(
    PortalDbContext context,
    PortalSeedData seed,
    IConfiguration configuration,
    IHostEnvironment environment,
    TimeProvider clock)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        // Production starts empty, because its real content is entered on the master page.
        if (environment.IsProduction() || await context.PortalTexts.AnyAsync(cancellationToken))
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // The starting birthdays and announcements count as posted today, so a new site shows them all.
        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

        await AddInOrderAsync(seed.HeroSlides, cancellationToken);
        await AddInOrderAsync(seed.CustomerFeedback, cancellationToken);
        await AddInOrderAsync(seed.Birthdays.Entries.Select(entry => entry with { PostedOn = today }), cancellationToken);
        await AddInOrderAsync(seed.RockstarAwards, cancellationToken);

        // The seed file lists the newest first, and these lists show the highest Id first.
        await AddInOrderAsync(seed.LatestNews.Reverse(), cancellationToken);
        await AddInOrderAsync(
            seed.WeddingBells.Announcements.Reverse().Select(text => new WeddingAnnouncement(0, text, today)),
            cancellationToken);
        await AddInOrderAsync(
            seed.NewBorns.Announcements.Reverse().Select(text => new NewBornAnnouncement(0, text, today)),
            cancellationToken);

        context.CeoDeskPosts.Add(new CeoDeskPost(0, seed.CeoDesk.Title, today, seed.CeoDesk.Message));
        context.TrainingSessions.AddRange(seed.TrainingSessions);
        context.MedicalInsurancePolicies.Add(seed.MedicalInsurance);

        context.PortalTexts.AddRange(
            new PortalText(PortalTextKey.BirthdaysPeriod, seed.Birthdays.Period),
            new PortalText(PortalTextKey.WeddingBellsClosing, seed.WeddingBells.Closing),
            new PortalText(PortalTextKey.NewBornClosing, seed.NewBorns.Closing),
            new PortalText(PortalTextKey.HolidayNotes, seed.HolidayNotes));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // One save per row, so that the Ids follow the list order.
    private async Task AddInOrderAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken)
        where T : class
    {
        foreach (var item in items)
        {
            context.Add(item);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
