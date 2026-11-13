namespace Itti.Inside.Domain.Content;

/// <summary>One slide of the landing page hero carousel.</summary>
public sealed record HeroSlide(int Id, string Title, string Subtitle) : IContentItem;

/// <summary>One announcement in the Latest News ticker.</summary>
public sealed record NewsItem(int Id, string Text) : IContentItem;

/// <summary>A piece of customer appreciation shown under Customer Feedback.</summary>
public sealed record CustomerFeedbackEntry(int Id, DateOnly Date, string Client, string Team, string Excerpt) : IContentItem;

/// <summary>The Birthdays card: a heading for the period plus who celebrates in it.</summary>
public sealed record BirthdayCalendar(string Period, IReadOnlyList<BirthdayEntry> Entries);

/// <summary>
/// One employee birthday. <paramref name="Day"/> is the day of the month. <paramref name="PostedOn"/> is
/// when the admin added it: birthdays posted this month are the new ones on the landing page.
/// </summary>
public sealed record BirthdayEntry(int Id, string EmployeeId, string Name, int Day, string Email, string Location, DateOnly PostedOn)
    : IContentItem;

/// <summary>A Rock Star Award announcement.</summary>
public sealed record RockstarAward(int Id, string Title, DateOnly PublishedOn, string Excerpt) : IContentItem
{
    /// <summary>
    /// The award's pictures in order, the first being its cover. Kept in <see cref="RockstarAwardImage"/>
    /// and filled in by the pages that show them.
    /// </summary>
    public IReadOnlyList<int> ImageIds { get; init; } = [];
}

/// <summary>One picture of a Rockstar Award. An award can have several, in the order of <paramref name="Position"/>.</summary>
public sealed record RockstarAwardImage(int Id, int AwardId, int ImageId, int Position);

public static class RockstarAwardRules
{
    public const int MaxImages = 10;
}

/// <summary>A congratulations board, used by both Wedding Bells and New Born.</summary>
public sealed record CelebrationBoard(IReadOnlyList<CelebrationAnnouncement> Announcements, string Closing);

/// <summary>One line on a congratulations board, with the day it was posted.</summary>
public sealed record CelebrationAnnouncement(int Id, string Text, DateOnly PostedOn);

/// <summary>One line on the Wedding Bells board. Lines posted this month are the new ones.</summary>
public sealed record WeddingAnnouncement(int Id, string Text, DateOnly PostedOn) : IContentItem;

/// <summary>One line on the New Born board. Lines posted this month are the new ones.</summary>
public sealed record NewBornAnnouncement(int Id, string Text, DateOnly PostedOn) : IContentItem;

/// <summary>A single piece of text that the master page can change.</summary>
public sealed record PortalText(PortalTextKey Key, string Value);
