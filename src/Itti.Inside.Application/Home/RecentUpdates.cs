namespace Itti.Inside.Application.Home;

/// <summary>What a landing page card shows, and whether it fell back to earlier updates.</summary>
/// <param name="Content">The card's content.</param>
/// <param name="ShowsEarlierUpdates">True when nothing was posted this month, so the card shows the latest earlier updates.</param>
public sealed record CardContent<T>(T Content, bool ShowsEarlierUpdates);

/// <summary>
/// The rule for the Birthdays, Wedding Bells and New Born cards: show what was posted this month.
/// When nothing was posted this month, show the last <see cref="FallbackCount"/> updates instead, so a
/// card never goes blank while there is something to show.
/// </summary>
public static class RecentUpdates
{
    public const int FallbackCount = 2;

    /// <param name="newestFirst">Every item, the most recently posted first.</param>
    /// <param name="postedOn">The day each item was posted.</param>
    /// <param name="today">Today, in the server's time zone.</param>
    public static CardContent<IReadOnlyList<T>> Select<T>(IEnumerable<T> newestFirst, Func<T, DateOnly> postedOn, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(newestFirst);
        ArgumentNullException.ThrowIfNull(postedOn);

        var items = newestFirst.ToList();
        var thisMonth = items.Where(item => IsSameMonth(postedOn(item), today)).ToList();

        return thisMonth.Count > 0 || items.Count == 0
            ? new CardContent<IReadOnlyList<T>>(thisMonth, ShowsEarlierUpdates: false)
            : new CardContent<IReadOnlyList<T>>(items.Take(FallbackCount).ToList(), ShowsEarlierUpdates: true);
    }

    private static bool IsSameMonth(DateOnly day, DateOnly today) => day.Year == today.Year && day.Month == today.Month;
}
