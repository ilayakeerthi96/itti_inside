using Itti.Inside.Application.Common;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Home;

/// <summary>Everything the landing page renders, in one object.</summary>
/// <remarks>
/// Birthdays, Wedding Bells and New Born hold this month's posts, or the latest earlier ones when
/// nothing was posted this month. See <see cref="RecentUpdates"/>.
/// </remarks>
public sealed record HomeDashboard(
    IReadOnlyList<HeroSlide> HeroSlides,
    IReadOnlyList<NewsItem> LatestNews,
    CeoDeskPost? CeoDesk,
    PagedResult<CustomerFeedbackEntry> CustomerFeedback,
    CardContent<BirthdayCalendar> Birthdays,
    PagedResult<RockstarAward> RockstarAwards,
    CardContent<CelebrationBoard> WeddingBells,
    CardContent<CelebrationBoard> NewBorns);
