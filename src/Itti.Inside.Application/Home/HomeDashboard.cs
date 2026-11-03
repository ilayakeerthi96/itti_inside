using Itti.Inside.Application.Common;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Home;

/// <summary>Everything the landing page renders, in one object.</summary>
public sealed record HomeDashboard(
    IReadOnlyList<HeroSlide> HeroSlides,
    IReadOnlyList<NewsItem> LatestNews,
    CeoMessage CeoMessage,
    PagedResult<CustomerFeedbackEntry> CustomerFeedback,
    BirthdayCalendar Birthdays,
    PagedResult<RockstarAward> RockstarAwards,
    CelebrationBoard WeddingBells,
    CelebrationBoard NewBorns);
