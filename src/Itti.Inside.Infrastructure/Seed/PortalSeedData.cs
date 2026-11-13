using System.Text.Json;
using Itti.Inside.Domain.Content;
using Itti.Inside.Domain.Navigation;

namespace Itti.Inside.Infrastructure.Seed;

/// <summary>
/// The dummy portal data, read once from the embedded <c>portal-content.json</c>.
/// It fills a new database, and still serves the menu, pages and applications.
/// </summary>
public sealed class PortalSeedData
{
    private const string ResourceName = "Itti.Inside.Infrastructure.Seed.portal-content.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public required IReadOnlyList<HeroSlide> HeroSlides { get; init; }

    public required IReadOnlyList<NewsItem> LatestNews { get; init; }

    public required SeedCeoDeskPost CeoDesk { get; init; }

    public required IReadOnlyList<CustomerFeedbackEntry> CustomerFeedback { get; init; }

    public required BirthdayCalendar Birthdays { get; init; }

    public required IReadOnlyList<RockstarAward> RockstarAwards { get; init; }

    public required SeedCelebrationBoard WeddingBells { get; init; }

    public required SeedCelebrationBoard NewBorns { get; init; }

    public required string HolidayNotes { get; init; }

    public required IReadOnlyList<TrainingSession> TrainingSessions { get; init; }

    public required MedicalInsurancePolicy MedicalInsurance { get; init; }

    public required IReadOnlyList<SeedMenuItem> Menu { get; init; }

    public required IReadOnlyList<PortalPage> Pages { get; init; }

    public required IReadOnlyList<PortalApplication> Applications { get; init; }

    public static PortalSeedData Load()
    {
        using var stream = typeof(PortalSeedData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

        return JsonSerializer.Deserialize<PortalSeedData>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' is empty.");
    }
}

/// <summary>The first CEO's Desk message, dated when it is added.</summary>
public sealed record SeedCeoDeskPost(string Title, string Message);

/// <summary>A congratulations board as written in the seed file: plain lines, dated when they are added.</summary>
public sealed record SeedCelebrationBoard(IReadOnlyList<string> Announcements, string Closing);

/// <summary>A menu entry as written in the seed file, where children and newTab are optional.</summary>
public sealed record SeedMenuItem(string Title, string? Path, IReadOnlyList<SeedMenuItem>? Children, bool NewTab = false)
{
    public NavigationItem ToNavigationItem() =>
        new(Title, Path, Children?.Select(child => child.ToNavigationItem()).ToArray() ?? [], NewTab);
}
