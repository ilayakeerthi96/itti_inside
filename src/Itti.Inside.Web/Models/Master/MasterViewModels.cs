using System.ComponentModel.DataAnnotations;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

/// <summary>One section of the master page. The main page sections come in the same order as on the page.</summary>
/// <param name="Group">The heading it sits under in the master page menu.</param>
/// <param name="TextKey">The single text value the section also edits, if any.</param>
/// <param name="LongText">True when that text is several lines long.</param>
public sealed record MasterSection(
    string Title,
    string Path,
    string Group,
    PortalTextKey? TextKey = null,
    string? TextLabel = null,
    bool LongText = false);

public static class MasterSections
{
    public const string MainPage = "Main page";
    public const string HrPages = "HR pages";

    public static IReadOnlyList<MasterSection> All { get; } =
    [
        new("Hero Slides", "hero-slides", MainPage),
        new("Latest News", "latest-news", MainPage),
        new("CEO's Desk", "ceo-desk", MainPage),
        new("Customer Feedback", "customer-feedback", MainPage),
        new("Birthdays", "birthdays", MainPage, PortalTextKey.BirthdaysPeriod, "Birthday period heading"),
        new("Rockstar Awards", "rockstar-awards", MainPage),
        new("Wedding Bells", "wedding-bells", MainPage, PortalTextKey.WeddingBellsClosing, "Closing line"),
        new("New Born", "new-born", MainPage, PortalTextKey.NewBornClosing, "Closing line"),
        new("Medical Insurance", "medical-insurance", HrPages),
        new("Holidays", "holidays", HrPages, PortalTextKey.HolidayNotes, "Notes under the holiday list, one per line", LongText: true),
        new("Organization Training", "training", HrPages),
    ];

    public static MasterSection ForText(PortalTextKey key) => All.First(section => section.TextKey == key);
}

/// <summary>A section's add or edit form, its current items and, on some sections, one text value.</summary>
public sealed record MasterSectionViewModel<TItem, TForm>(IReadOnlyList<TItem> Items, TForm Form, MasterTextForm? Text)
    where TForm : IContentForm<TItem>
{
    public bool IsEditing => Form.Id != 0;
}

/// <summary>The upload panel of a section that offers a file to employees.</summary>
/// <param name="Accept">The file types, as the file input's accept list, for example ".docx,.pdf".</param>
/// <param name="OffersListUpdate">True on Holidays, where a Word file can also fill the holiday list.</param>
public sealed record DocumentPanelModel(string Title, string Help, string Accept, PortalDocumentInfo? Document, bool OffersListUpdate = false);

public sealed class MasterTextForm
{
    public PortalTextKey Key { get; set; }

    [Required]
    [StringLength(ContentLimits.LongText)]
    public string Value { get; set; } = string.Empty;
}

public static class MasterNotice
{
    /// <summary>The TempData entry that carries a message across the redirect after a save.</summary>
    public const string Key = nameof(MasterNotice);

    public const string Saved = "Saved. Employees now see this on the portal.";

    public const string Deleted = "Deleted. Employees no longer see it on the portal.";

    public const string FileDeleted = "File removed. Employees can no longer download it.";

    public const string Missing = "This item was already deleted, so nothing was saved.";

    public const string TextNotSaved = "Nothing was saved. Enter the text, up to 4000 characters.";
}
