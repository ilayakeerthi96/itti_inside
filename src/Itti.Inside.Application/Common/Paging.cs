namespace Itti.Inside.Application.Common;

/// <summary>A page of results, with the requested page clamped to what actually exists.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}

public static class Paging
{
    /// <summary>Moves an out of range page to the nearest valid page.</summary>
    public static int ClampPage(int page, int pageSize, int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var lastPage = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        return Math.Clamp(page, 1, lastPage);
    }
}
