using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Web.Models.Master;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers.Master;

[Route("master/hero-slides")]
public sealed class HeroSlidesMasterController(IPortalContentEditor editor, ILogger<HeroSlidesMasterController> logger)
    : MasterSectionController<HeroSlide, HeroSlideForm>(editor, logger)
{
    protected override string SectionView => "HeroSlides";

    protected override IEnumerable<HeroSlide> Arrange(IEnumerable<HeroSlide> items) => items.OrderBy(slide => slide.Id);
}

[Route("master/latest-news")]
public sealed class LatestNewsMasterController(IPortalContentEditor editor, ILogger<LatestNewsMasterController> logger)
    : MasterSectionController<NewsItem, NewsItemForm>(editor, logger)
{
    protected override string SectionView => "LatestNews";
}

[Route("master/customer-feedback")]
public sealed class CustomerFeedbackMasterController(IPortalContentEditor editor, ILogger<CustomerFeedbackMasterController> logger)
    : MasterSectionController<CustomerFeedbackEntry, CustomerFeedbackForm>(editor, logger)
{
    protected override string SectionView => "CustomerFeedback";

    protected override IEnumerable<CustomerFeedbackEntry> Arrange(IEnumerable<CustomerFeedbackEntry> items) =>
        items.OrderByDescending(entry => entry.Date);
}

[Route("master/birthdays")]
public sealed class BirthdaysMasterController(IPortalContentEditor editor, ILogger<BirthdaysMasterController> logger)
    : MasterSectionController<BirthdayEntry, BirthdayForm>(editor, logger)
{
    protected override string SectionView => "Birthdays";

    protected override PortalTextKey? TextKey => PortalTextKey.BirthdaysPeriod;

    protected override IEnumerable<BirthdayEntry> Arrange(IEnumerable<BirthdayEntry> items) =>
        items.OrderBy(entry => entry.Day);
}

[Route("master/rockstar-awards")]
public sealed class RockstarAwardsMasterController(IPortalContentEditor editor, ILogger<RockstarAwardsMasterController> logger)
    : MasterSectionController<RockstarAward, RockstarAwardForm>(editor, logger)
{
    protected override string SectionView => "RockstarAwards";

    protected override IEnumerable<RockstarAward> Arrange(IEnumerable<RockstarAward> items) =>
        items.OrderByDescending(award => award.PublishedOn);
}

[Route("master/wedding-bells")]
public sealed class WeddingBellsMasterController(IPortalContentEditor editor, ILogger<WeddingBellsMasterController> logger)
    : MasterSectionController<WeddingAnnouncement, WeddingAnnouncementForm>(editor, logger)
{
    protected override string SectionView => "WeddingBells";

    protected override PortalTextKey? TextKey => PortalTextKey.WeddingBellsClosing;
}

[Route("master/new-born")]
public sealed class NewBornMasterController(IPortalContentEditor editor, ILogger<NewBornMasterController> logger)
    : MasterSectionController<NewBornAnnouncement, NewBornAnnouncementForm>(editor, logger)
{
    protected override string SectionView => "NewBorn";

    protected override PortalTextKey? TextKey => PortalTextKey.NewBornClosing;
}
