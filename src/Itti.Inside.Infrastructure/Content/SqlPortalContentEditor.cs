using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Itti.Inside.Infrastructure.Content;

/// <summary>Saves master page changes to the database.</summary>
public sealed class SqlPortalContentEditor(PortalDbContext context) : IPortalContentEditor
{
    public async Task<IReadOnlyList<T>> ListAsync<T>(CancellationToken cancellationToken)
        where T : class, IContentItem =>
        await context.Set<T>()
            .OrderByDescending(item => EF.Property<int>(item, nameof(IContentItem.Id)))
            .ToListAsync(cancellationToken);

    public Task<T?> FindAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IContentItem =>
        context.Set<T>().FirstOrDefaultAsync(item => EF.Property<int>(item, nameof(IContentItem.Id)) == id, cancellationToken);

    public async Task<bool> SaveAsync<T>(T item, CancellationToken cancellationToken)
        where T : class, IContentItem
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Id == 0)
        {
            context.Add(item);
        }
        else
        {
            context.Update(item);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else deleted the item while this form was open.
            return false;
        }
    }

    public Task DeleteAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IContentItem =>
        context.Set<T>()
            .Where(item => EF.Property<int>(item, nameof(IContentItem.Id)) == id)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<string> GetTextAsync(PortalTextKey key, CancellationToken cancellationToken) =>
        await context.PortalTexts
            .Where(text => text.Key == key)
            .Select(text => text.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

    public async Task SetTextAsync(PortalTextKey key, string value, CancellationToken cancellationToken)
    {
        var exists = await context.PortalTexts.AnyAsync(text => text.Key == key, cancellationToken);

        context.Entry(new PortalText(key, value)).State = exists ? EntityState.Modified : EntityState.Added;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceHolidaysAsync(IReadOnlyList<Holiday> holidays, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        var years = holidays.Select(holiday => holiday.Date.Year).Distinct().ToList();

        // Both steps or neither, so a failed upload never leaves a year half replaced.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var year in years)
        {
            var first = new DateOnly(year, 1, 1);
            var last = new DateOnly(year, 12, 31);

            await context.Holidays
                .Where(holiday => holiday.Date >= first && holiday.Date <= last)
                .ExecuteDeleteAsync(cancellationToken);
        }

        context.Holidays.AddRange(holidays.Select(holiday => holiday with { Id = 0 }));
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SaveDocumentAsync(PortalDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var exists = await context.PortalDocuments.AnyAsync(stored => stored.Key == document.Key, cancellationToken);

        context.Entry(document).State = exists ? EntityState.Modified : EntityState.Added;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteDocumentAsync(string key, CancellationToken cancellationToken) =>
        context.PortalDocuments.Where(document => document.Key == key).ExecuteDeleteAsync(cancellationToken);

    public async Task<int> SaveImageAsync(PortalImage image, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image);

        context.PortalImages.Add(image);
        await context.SaveChangesAsync(cancellationToken);

        return image.Id;
    }

    public Task DeleteImageAsync(int id, CancellationToken cancellationToken) =>
        context.PortalImages.Where(image => image.Id == id).ExecuteDeleteAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetAwardImageIdsAsync(int awardId, CancellationToken cancellationToken) =>
        await context.RockstarAwardImages
            .Where(link => link.AwardId == awardId)
            .OrderBy(link => link.Position)
            .ThenBy(link => link.Id)
            .Select(link => link.ImageId)
            .ToListAsync(cancellationToken);

    public async Task SetAwardImagesAsync(int awardId, IReadOnlyList<int> imageIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(imageIds);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.RockstarAwardImages.Where(link => link.AwardId == awardId).ExecuteDeleteAsync(cancellationToken);
        context.RockstarAwardImages.AddRange(imageIds.Select((imageId, position) => new RockstarAwardImage(0, awardId, imageId, position)));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
