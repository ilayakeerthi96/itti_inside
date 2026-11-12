namespace Itti.Inside.Domain.Content;

/// <summary>A message on the CEO's Desk. The landing page shows the newest; its page shows them all.</summary>
/// <param name="ImageId">The message's picture, if it has one. See <see cref="PortalImage"/>.</param>
public sealed record CeoDeskPost(int Id, string Title, DateOnly PostedOn, string Message, int? ImageId = null) : IContentItem, IHasImage;

/// <summary>One company holiday. An optional holiday is one that employees may choose to take.</summary>
public sealed record Holiday(int Id, DateOnly Date, string Name, bool IsOptional) : IContentItem;

/// <summary>A training session on the Organization Training page.</summary>
/// <param name="Category">The kind of training, one of <see cref="TrainingCategories"/>.</param>
/// <param name="Time">Free text, for example "10:00 AM – 1:00 PM".</param>
/// <param name="Mode">How it is held, one of <see cref="TrainingModes"/>.</param>
/// <param name="Venue">A room, or the address of an online meeting.</param>
/// <param name="Registration">How to sign up: a web address, an email address or a short instruction.</param>
public sealed record TrainingSession(
    int Id,
    string Title,
    string Category,
    DateOnly Date,
    string Time,
    string Mode,
    string Venue,
    string Trainer,
    string Audience,
    string Description,
    string Registration) : IContentItem;

public static class TrainingCategories
{
    public static IReadOnlyList<string> All { get; } =
        ["Technical", "Soft skills", "Leadership", "Compliance", "Induction", "Other"];
}

public static class TrainingModes
{
    public static IReadOnlyList<string> All { get; } = ["Classroom", "Online", "Hybrid"];
}

/// <summary>
/// The company's group medical insurance, shown on the Corporate Medical Insurance page. There is one,
/// edited as a whole on the master page.
/// </summary>
/// <param name="Administrator">The third party administrator (TPA) that handles claims, if any.</param>
/// <param name="SumInsured">Free text, for example "₹5,00,000 per family per year".</param>
/// <param name="Coverage">Who is covered, for example "Employee, spouse and two children".</param>
/// <param name="Benefits">One benefit per line.</param>
/// <param name="ClaimSteps">One step of the claim process per line.</param>
public sealed record MedicalInsurancePolicy(
    int Id,
    string Insurer,
    string Administrator,
    string PolicyNumber,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    string SumInsured,
    string Coverage,
    string Benefits,
    string ClaimSteps,
    string NetworkHospitalsLink,
    string HelpdeskName,
    string HelpdeskPhone,
    string HelpdeskEmail) : IContentItem;

/// <summary>A file the admin uploads for employees to download, such as the holiday list.</summary>
public sealed class PortalDocument
{
    public required string Key { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }

    public required DateTimeOffset UploadedOn { get; init; }
}

/// <summary>What a page shows about an uploaded file, without loading the file itself.</summary>
public sealed record PortalDocumentInfo(string Key, string FileName, long Size, DateTimeOffset UploadedOn);

/// <summary>The upload slots, and the file types each accepts.</summary>
public static class PortalDocumentKeys
{
    public const string HolidayList = "holiday-list";
    public const string MedicalInsurancePolicy = "medical-insurance-policy";

    public const int MaxBytes = 10 * 1024 * 1024;

    public static IReadOnlyDictionary<string, string> ContentTypes { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    };

    public static bool IsKnown(string key) => key is HolidayList or MedicalInsurancePolicy;
}
