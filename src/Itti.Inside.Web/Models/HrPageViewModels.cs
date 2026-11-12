using System.Globalization;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models;

public sealed record MedicalInsurancePageViewModel(MedicalInsurancePolicy? Policy, PortalDocumentInfo? Document);

/// <param name="Year">The year on show.</param>
/// <param name="Years">Every year with holidays, for the year links.</param>
/// <param name="Holidays">The year's holidays in date order.</param>
public sealed record HolidaysPageViewModel(
    int Year,
    IReadOnlyList<int> Years,
    IReadOnlyList<Holiday> Holidays,
    string Notes,
    PortalDocumentInfo? Document,
    DateOnly Today)
{
    /// <summary>The next holiday from today, if it is in this year's list.</summary>
    public Holiday? Next => Holidays.FirstOrDefault(holiday => holiday.Date >= Today);
}

/// <param name="Upcoming">Today and later, soonest first.</param>
/// <param name="Past">Before today, most recent first.</param>
public sealed record TrainingPageViewModel(IReadOnlyList<TrainingSession> Upcoming, IReadOnlyList<TrainingSession> Past);

public static class FileSizeDisplay
{
    /// <summary>"240 KB" or "1.4 MB", as people expect to read a file size.</summary>
    public static string Format(long bytes) =>
        bytes < 1024 * 1024
            ? $"{Math.Max(1, (int)Math.Round(bytes / 1024.0))} KB"
            : (bytes / (1024.0 * 1024.0)).ToString("0.0 'MB'", CultureInfo.InvariantCulture);
}
