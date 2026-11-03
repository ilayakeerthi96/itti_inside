using System.ComponentModel.DataAnnotations;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

/// <summary>A master page form for one kind of content. An Id of 0 means a new item.</summary>
public interface IContentForm<TItem>
{
    int Id { get; set; }

    void Load(TItem item);

    TItem ToItem();
}

public sealed class HeroSlideForm : IContentForm<HeroSlide>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Subtitle { get; set; } = string.Empty;

    public void Load(HeroSlide item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Title, Subtitle) = item;
    }

    public HeroSlide ToItem() => new(Id, Title.Trim(), Subtitle.Trim());
}

public sealed class NewsItemForm : IContentForm<NewsItem>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.LongText)]
    [Display(Name = "News")]
    public string Text { get; set; } = string.Empty;

    public void Load(NewsItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Text) = item;
    }

    public NewsItem ToItem() => new(Id, Text.Trim());
}

public sealed class CustomerFeedbackForm : IContentForm<CustomerFeedbackEntry>
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Client { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Team { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.LongText)]
    [Display(Name = "Feedback")]
    public string Excerpt { get; set; } = string.Empty;

    public void Load(CustomerFeedbackEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Date, Client, Team, Excerpt) = item;
    }

    public CustomerFeedbackEntry ToItem() => new(Id, Date, Client.Trim(), Team.Trim(), Excerpt.Trim());
}

public sealed class BirthdayForm : IContentForm<BirthdayEntry>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.EmployeeId)]
    [Display(Name = "Employee ID")]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 31)]
    [Display(Name = "Day of the month")]
    public int Day { get; set; } = 1;

    [Required]
    [EmailAddress]
    [StringLength(ContentLimits.Email)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Location { get; set; } = string.Empty;

    public void Load(BirthdayEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, EmployeeId, Name, Day, Email, Location) = item;
    }

    public BirthdayEntry ToItem() => new(Id, EmployeeId.Trim(), Name.Trim(), Day, Email.Trim(), Location.Trim());
}

public sealed class RockstarAwardForm : IContentForm<RockstarAward>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Published on")]
    public DateOnly PublishedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [StringLength(ContentLimits.LongText)]
    [Display(Name = "Announcement")]
    public string Excerpt { get; set; } = string.Empty;

    public void Load(RockstarAward item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Title, PublishedOn, Excerpt) = item;
    }

    public RockstarAward ToItem() => new(Id, Title.Trim(), PublishedOn, Excerpt.Trim());
}

public sealed class WeddingAnnouncementForm : IContentForm<WeddingAnnouncement>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Announcement")]
    public string Text { get; set; } = string.Empty;

    public void Load(WeddingAnnouncement item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Text) = item;
    }

    public WeddingAnnouncement ToItem() => new(Id, Text.Trim());
}

public sealed class NewBornAnnouncementForm : IContentForm<NewBornAnnouncement>
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Announcement")]
    public string Text { get; set; } = string.Empty;

    public void Load(NewBornAnnouncement item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Text) = item;
    }

    public NewBornAnnouncement ToItem() => new(Id, Text.Trim());
}
