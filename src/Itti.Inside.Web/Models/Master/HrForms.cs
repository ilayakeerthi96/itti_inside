using System.ComponentModel.DataAnnotations;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

public sealed class CeoDeskPostForm : IContentForm<CeoDeskPost>, IImageForm
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Posted on")]
    public DateOnly PostedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [StringLength(ContentLimits.Letter)]
    public string Message { get; set; } = string.Empty;

    public int? ImageId { get; set; }

    [Display(Name = "Picture")]
    public IFormFile? Image { get; set; }

    [Display(Name = "Remove the picture")]
    public bool RemoveImage { get; set; }

    public void Load(CeoDeskPost item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Title, PostedOn, Message, ImageId) = item;
    }

    public CeoDeskPost ToItem() => new(Id, Title.Trim(), PostedOn, Message.Trim(), ImageId);
}

public sealed class HolidayForm : IContentForm<Holiday>
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Holiday")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Optional holiday")]
    public bool IsOptional { get; set; }

    public void Load(Holiday item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Date, Name, IsOptional) = item;
    }

    public Holiday ToItem() => new(Id, Date, Name.Trim(), IsOptional);
}

public sealed class TrainingSessionForm : IContentForm<TrainingSession>, IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = TrainingCategories.All[0];

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(ContentLimits.Label)]
    public string? Time { get; set; }

    [Required]
    public string Mode { get; set; } = TrainingModes.All[0];

    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Venue or meeting link")]
    public string? Venue { get; set; }

    [StringLength(ContentLimits.ShortText)]
    public string? Trainer { get; set; }

    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Who should attend")]
    public string? Audience { get; set; }

    [StringLength(ContentLimits.LongText)]
    [Display(Name = "About the session")]
    public string? Description { get; set; }

    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "How to register")]
    public string? Registration { get; set; }

    public void Load(TrainingSession item)
    {
        ArgumentNullException.ThrowIfNull(item);

        (Id, Title, Category, Date, Time, Mode, Venue, Trainer, Audience, Description, Registration) = item;
    }

    public TrainingSession ToItem() =>
        new(Id, Title.Trim(), Category, Date, Clean(Time), Mode, Clean(Venue), Clean(Trainer), Clean(Audience), Clean(Description), Clean(Registration));

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TrainingCategories.All.Contains(Category))
        {
            yield return new ValidationResult("Choose a category from the list.", [nameof(Category)]);
        }

        if (!TrainingModes.All.Contains(Mode))
        {
            yield return new ValidationResult("Choose how the session is held.", [nameof(Mode)]);
        }
    }

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;
}

/// <summary>The one medical insurance record, edited as a whole.</summary>
public sealed class MedicalInsuranceForm : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Insurance company")]
    public string Insurer { get; set; } = string.Empty;

    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Claims administrator (TPA)")]
    public string? Administrator { get; set; }

    [StringLength(ContentLimits.Label)]
    [Display(Name = "Policy number")]
    public string? PolicyNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Valid from")]
    public DateOnly ValidFrom { get; set; } = new(DateTime.Today.Year, 4, 1);

    [DataType(DataType.Date)]
    [Display(Name = "Valid to")]
    public DateOnly ValidTo { get; set; } = new(DateTime.Today.Year + 1, 3, 31);

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Sum insured")]
    public string SumInsured { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Who is covered")]
    public string Coverage { get; set; } = string.Empty;

    [StringLength(ContentLimits.LongText)]
    [Display(Name = "Key benefits, one per line")]
    public string? Benefits { get; set; }

    [StringLength(ContentLimits.LongText)]
    [Display(Name = "How to make a claim, one step per line")]
    public string? ClaimSteps { get; set; }

    [StringLength(ContentLimits.Address)]
    [Display(Name = "Network hospitals link")]
    public string? NetworkHospitalsLink { get; set; }

    [StringLength(ContentLimits.ShortText)]
    [Display(Name = "Helpdesk contact")]
    public string? HelpdeskName { get; set; }

    [StringLength(ContentLimits.Label)]
    [Display(Name = "Helpdesk phone")]
    public string? HelpdeskPhone { get; set; }

    [EmailAddress]
    [StringLength(ContentLimits.Email)]
    [Display(Name = "Helpdesk email")]
    public string? HelpdeskEmail { get; set; }

    public void Load(MedicalInsurancePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        (Id, Insurer, Administrator, PolicyNumber, ValidFrom, ValidTo, SumInsured, Coverage, Benefits, ClaimSteps,
            NetworkHospitalsLink, HelpdeskName, HelpdeskPhone, HelpdeskEmail) = policy;
    }

    public MedicalInsurancePolicy ToItem() =>
        new(Id, Insurer.Trim(), Clean(Administrator), Clean(PolicyNumber), ValidFrom, ValidTo, SumInsured.Trim(), Coverage.Trim(),
            Clean(Benefits), Clean(ClaimSteps), Clean(NetworkHospitalsLink), Clean(HelpdeskName), Clean(HelpdeskPhone), Clean(HelpdeskEmail));

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ValidTo < ValidFrom)
        {
            yield return new ValidationResult("The end date must be on or after the start date.", [nameof(ValidTo)]);
        }

        if (!string.IsNullOrWhiteSpace(NetworkHospitalsLink) && !LinkDisplay.IsWebAddress(NetworkHospitalsLink.Trim()))
        {
            yield return new ValidationResult("Enter a full web address, starting with https://.", [nameof(NetworkHospitalsLink)]);
        }
    }

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;
}
