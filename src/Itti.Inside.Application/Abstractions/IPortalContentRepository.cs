using Itti.Inside.Application.Common;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Abstractions;

/// <summary>Everything the portal pages show. <see cref="IPortalContentEditor"/> is the write side.</summary>
public interface IPortalContentRepository
{
    Task<IReadOnlyList<HeroSlide>> GetHeroSlidesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsItem>> GetLatestNewsAsync(CancellationToken cancellationToken);

    /// <summary>Every CEO's Desk message, newest first. The landing page shows the first.</summary>
    Task<IReadOnlyList<CeoDeskPost>> GetCeoDeskPostsAsync(CancellationToken cancellationToken);

    Task<PagedResult<CustomerFeedbackEntry>> GetCustomerFeedbackAsync(int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Every piece of feedback, newest first, for the Customer Feedback page.</summary>
    Task<IReadOnlyList<CustomerFeedbackEntry>> GetAllCustomerFeedbackAsync(CancellationToken cancellationToken);

    /// <summary>Every birthday, newest posted first, then by day of the month.</summary>
    Task<BirthdayCalendar> GetBirthdaysAsync(CancellationToken cancellationToken);

    Task<PagedResult<RockstarAward>> GetRockstarAwardsAsync(int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Every award, newest first, for the Rockstar Awards page.</summary>
    Task<IReadOnlyList<RockstarAward>> GetAllRockstarAwardsAsync(CancellationToken cancellationToken);

    /// <summary>Every Wedding Bells line, newest posted first.</summary>
    Task<CelebrationBoard> GetWeddingBellsAsync(CancellationToken cancellationToken);

    /// <summary>Every New Born line, newest posted first.</summary>
    Task<CelebrationBoard> GetNewBornsAsync(CancellationToken cancellationToken);

    /// <summary>Every holiday, in date order.</summary>
    Task<IReadOnlyList<Holiday>> GetHolidaysAsync(CancellationToken cancellationToken);

    Task<string> GetHolidayNotesAsync(CancellationToken cancellationToken);

    /// <summary>Every training session, in date order.</summary>
    Task<IReadOnlyList<TrainingSession>> GetTrainingSessionsAsync(CancellationToken cancellationToken);

    /// <summary>The company's medical insurance, or null before the admin enters it.</summary>
    Task<MedicalInsurancePolicy?> GetMedicalInsuranceAsync(CancellationToken cancellationToken);

    /// <summary>The name, size and date of an uploaded file, or null if there is none.</summary>
    Task<PortalDocumentInfo?> GetDocumentInfoAsync(string key, CancellationToken cancellationToken);

    /// <summary>An uploaded file with its content, for download.</summary>
    Task<PortalDocument?> GetDocumentAsync(string key, CancellationToken cancellationToken);

    /// <summary>A CEO's Desk or Rockstar Award picture, for display.</summary>
    Task<PortalImage?> GetImageAsync(int id, CancellationToken cancellationToken);
}
