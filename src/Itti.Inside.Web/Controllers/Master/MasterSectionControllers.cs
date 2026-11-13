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
        items.OrderByDescending(entry => entry.PostedOn).ThenBy(entry => entry.Day);
}

[Route("master/rockstar-awards")]
public sealed class RockstarAwardsMasterController(
    IPortalContentEditor editor,
    IPortalContentRepository content,
    TimeProvider clock,
    ILogger<RockstarAwardsMasterController> logger)
    : MasterSectionController<RockstarAward, RockstarAwardForm>(editor, logger)
{
    // The pictures stored for this save, so they can go again if the save doesn't happen.
    private readonly List<int> _added = [];

    protected override string SectionView => "RockstarAwards";

    protected override IEnumerable<RockstarAward> Arrange(IEnumerable<RockstarAward> items) =>
        items.OrderByDescending(award => award.PublishedOn);

    // The list shows each award's cover picture.
    protected override async Task AddViewDataAsync(CancellationToken cancellationToken) =>
        ViewData[nameof(RockstarAward.ImageIds)] = (await content.GetAllRockstarAwardsAsync(cancellationToken))
            .Where(award => award.ImageIds.Count > 0)
            .ToDictionary(award => award.Id, award => award.ImageIds);

    protected override async Task PrepareFormAsync(RockstarAwardForm form, CancellationToken cancellationToken) =>
        form.ImageIds = form.Id == 0 ? [] : await Editor.GetAwardImageIdsAsync(form.Id, cancellationToken);

    protected override async Task<bool> BeforeSaveAsync(RockstarAwardForm form, CancellationToken cancellationToken)
    {
        var uploads = form.Images.Where(file => file.Length > 0).ToList();
        var kept = form.ImageIds.Count(id => !form.RemoveImageIds.Contains(id));

        if (kept + uploads.Count > RockstarAwardRules.MaxImages)
        {
            ModelState.AddModelError("Form.Images", $"An award can have up to {RockstarAwardRules.MaxImages} pictures. Remove some, or choose fewer.");
            return false;
        }

        foreach (var file in uploads)
        {
            var (image, problem) = await ImageUpload.ReadAsync(file, clock.GetUtcNow(), cancellationToken);

            if (image is null)
            {
                ModelState.AddModelError("Form.Images", $"{Path.GetFileName(file.FileName)}: {problem}");
                await DeleteAddedAsync(cancellationToken);
                return false;
            }

            _added.Add(await Editor.SaveImageAsync(image, cancellationToken));
        }

        return true;
    }

    protected override async Task AfterSaveAsync(RockstarAwardForm form, RockstarAward item, bool saved, CancellationToken cancellationToken)
    {
        if (!saved)
        {
            await DeleteAddedAsync(cancellationToken);
            return;
        }

        var removed = form.ImageIds.Where(form.RemoveImageIds.Contains).ToList();
        var pictures = form.ImageIds.Except(removed).Concat(_added).ToList();

        await Editor.SetAwardImagesAsync(item.Id, pictures, cancellationToken);

        foreach (var imageId in removed)
        {
            await Editor.DeleteImageAsync(imageId, cancellationToken);
        }
    }

    // The award's pictures go with it.
    protected override async Task BeforeDeleteAsync(int id, CancellationToken cancellationToken)
    {
        foreach (var imageId in await Editor.GetAwardImageIdsAsync(id, cancellationToken))
        {
            await Editor.DeleteImageAsync(imageId, cancellationToken);
        }
    }

    private async Task DeleteAddedAsync(CancellationToken cancellationToken)
    {
        foreach (var imageId in _added)
        {
            await Editor.DeleteImageAsync(imageId, cancellationToken);
        }

        _added.Clear();
    }
}

[Route("master/wedding-bells")]
public sealed class WeddingBellsMasterController(IPortalContentEditor editor, ILogger<WeddingBellsMasterController> logger)
    : MasterSectionController<WeddingAnnouncement, WeddingAnnouncementForm>(editor, logger)
{
    protected override string SectionView => "WeddingBells";

    protected override PortalTextKey? TextKey => PortalTextKey.WeddingBellsClosing;

    protected override IEnumerable<WeddingAnnouncement> Arrange(IEnumerable<WeddingAnnouncement> items) =>
        items.OrderByDescending(item => item.PostedOn).ThenByDescending(item => item.Id);
}

[Route("master/new-born")]
public sealed class NewBornMasterController(IPortalContentEditor editor, ILogger<NewBornMasterController> logger)
    : MasterSectionController<NewBornAnnouncement, NewBornAnnouncementForm>(editor, logger)
{
    protected override string SectionView => "NewBorn";

    protected override PortalTextKey? TextKey => PortalTextKey.NewBornClosing;

    protected override IEnumerable<NewBornAnnouncement> Arrange(IEnumerable<NewBornAnnouncement> items) =>
        items.OrderByDescending(item => item.PostedOn).ThenByDescending(item => item.Id);
}
