using Itti.Inside.Application.Common;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Abstractions;

/// <summary>Everything the landing page shows. <see cref="IPortalContentEditor"/> is the write side.</summary>
public interface IPortalContentRepository
{
    Task<IReadOnlyList<HeroSlide>> GetHeroSlidesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsItem>> GetLatestNewsAsync(CancellationToken cancellationToken);

    Task<CeoMessage> GetCeoMessageAsync(CancellationToken cancellationToken);

    Task<PagedResult<CustomerFeedbackEntry>> GetCustomerFeedbackAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<BirthdayCalendar> GetBirthdaysAsync(CancellationToken cancellationToken);

    Task<PagedResult<RockstarAward>> GetRockstarAwardsAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<CelebrationBoard> GetWeddingBellsAsync(CancellationToken cancellationToken);

    Task<CelebrationBoard> GetNewBornsAsync(CancellationToken cancellationToken);
}
