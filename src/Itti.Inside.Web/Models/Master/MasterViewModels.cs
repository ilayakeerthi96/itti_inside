using System.ComponentModel.DataAnnotations;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

/// <summary>One section of the master page, in the same order as the main page.</summary>
public sealed record MasterSection(string Title, string Path, PortalTextKey? TextKey = null, string? TextLabel = null);

public static class MasterSections
{
    public static IReadOnlyList<MasterSection> All { get; } =
    [
        new("Hero Slides", "hero-slides"),
        new("Latest News", "latest-news"),
        new("CEO's Desk", "ceo-desk", PortalTextKey.CeoMessage, "CEO's message"),
        new("Customer Feedback", "customer-feedback"),
        new("Birthdays", "birthdays", PortalTextKey.BirthdaysPeriod, "Birthday period heading"),
        new("Rockstar Awards", "rockstar-awards"),
        new("Wedding Bells", "wedding-bells", PortalTextKey.WeddingBellsClosing, "Closing line"),
        new("New Born", "new-born", PortalTextKey.NewBornClosing, "Closing line"),
    ];

    public static MasterSection ForText(PortalTextKey key) => All.First(section => section.TextKey == key);
}

/// <summary>A section's add or edit form, its current items and, on some sections, one text value.</summary>
public sealed record MasterSectionViewModel<TItem, TForm>(IReadOnlyList<TItem> Items, TForm Form, MasterTextForm? Text)
    where TForm : IContentForm<TItem>
{
    public bool IsEditing => Form.Id != 0;
}

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

    public const string Saved = "Saved. Employees now see this on the main page.";

    public const string Deleted = "Deleted. Employees no longer see it on the main page.";

    public const string Missing = "This item was already deleted, so nothing was saved.";

    public const string TextNotSaved = "Nothing was saved. Enter the text, up to 4000 characters.";
}
