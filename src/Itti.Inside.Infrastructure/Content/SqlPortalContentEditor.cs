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
}
