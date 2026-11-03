using Itti.Inside.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Itti.Inside.Infrastructure.Persistence;

internal static class ContentPaging
{
    /// <summary>Fetches one page in the database. Out of range pages fall back to the nearest valid page.</summary>
    public static async Task<PagedResult<T>> ToPageAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var current = Paging.ClampPage(page, pageSize, totalCount);
        var items = await query.Skip((current - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, current, pageSize, totalCount);
    }
}
