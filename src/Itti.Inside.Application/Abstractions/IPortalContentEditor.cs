using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Abstractions;

/// <summary>What the master page changes. Every save shows on the landing page straight away.</summary>
public interface IPortalContentEditor
{
    /// <summary>All items of one kind, newest first.</summary>
    Task<IReadOnlyList<T>> ListAsync<T>(CancellationToken cancellationToken)
        where T : class, IContentItem;

    Task<T?> FindAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IContentItem;

    /// <summary>Adds the item when its Id is 0, otherwise replaces it. False means it no longer exists.</summary>
    Task<bool> SaveAsync<T>(T item, CancellationToken cancellationToken)
        where T : class, IContentItem;

    Task DeleteAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IContentItem;

    Task<string> GetTextAsync(PortalTextKey key, CancellationToken cancellationToken);

    Task SetTextAsync(PortalTextKey key, string value, CancellationToken cancellationToken);
}
