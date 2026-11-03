namespace Itti.Inside.Domain.Content;

/// <summary>One slide of the landing page hero carousel.</summary>
public sealed record HeroSlide(int Id, string Title, string Subtitle) : IContentItem;

/// <summary>One announcement in the Latest News ticker.</summary>
public sealed record NewsItem(int Id, string Text) : IContentItem;

/// <summary>The current message shown on the CEO's Desk card.</summary>
public sealed record CeoMessage(string Excerpt);

/// <summary>A piece of customer appreciation shown under Customer Feedback.</summary>
public sealed record CustomerFeedbackEntry(int Id, DateOnly Date, string Client, string Team, string Excerpt) : IContentItem;

/// <summary>The Birthdays card: a heading for the period plus who celebrates in it.</summary>
public sealed record BirthdayCalendar(string Period, IReadOnlyList<BirthdayEntry> Entries);

/// <summary>One employee birthday. <paramref name="Day"/> is the day of the month.</summary>
public sealed record BirthdayEntry(int Id, string EmployeeId, string Name, int Day, string Email, string Location) : IContentItem;

/// <summary>A Rock Star Award announcement.</summary>
public sealed record RockstarAward(int Id, string Title, DateOnly PublishedOn, string Excerpt) : IContentItem;

/// <summary>A congratulations board, used by both Wedding Bells and New Born.</summary>
public sealed record CelebrationBoard(IReadOnlyList<string> Announcements, string Closing);

/// <summary>One line on the Wedding Bells board.</summary>
public sealed record WeddingAnnouncement(int Id, string Text) : IContentItem;

/// <summary>One line on the New Born board.</summary>
public sealed record NewBornAnnouncement(int Id, string Text) : IContentItem;

/// <summary>A single piece of text that the master page can change.</summary>
public sealed record PortalText(PortalTextKey Key, string Value);
