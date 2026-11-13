namespace Itti.Inside.Domain.Content;

/// <summary>Anything the master page can add, change and delete. An Id of 0 means not saved yet.</summary>
public interface IContentItem
{
    int Id { get; }
}

/// <summary>The page text that is one value rather than a list. Stored by name, so keep the names.</summary>
public enum PortalTextKey
{
    BirthdaysPeriod,
    WeddingBellsClosing,
    NewBornClosing,

    /// <summary>The notes under the holiday list, one per line.</summary>
    HolidayNotes,
}

/// <summary>Maximum lengths, shared by the master page forms and the database columns.</summary>
public static class ContentLimits
{
    public const int EmployeeId = 20;
    public const int Label = 50;
    public const int Email = 254;
    public const int ShortText = 200;
    public const int Address = 500;
    public const int LongText = 4000;

    /// <summary>
    /// The CEO's Desk message, stored as nvarchar(MAX). The form stops at about 40 pages of text, so a
    /// mistaken paste can't fill the database.
    /// </summary>
    public const int Letter = 100_000;
}
