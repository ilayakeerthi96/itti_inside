using System.Security.Claims;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Web.Models.Master;
using Itti.Inside.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers.Master;

/// <summary>List, add, edit and delete for one master page section.</summary>
[Authorize(Policy = PortalPolicies.ManageContent)]
public abstract class MasterSectionController<TItem, TForm>(IPortalContentEditor editor, ILogger logger) : Controller
    where TItem : class, IContentItem
    where TForm : class, IContentForm<TItem>, new()
{
    /// <summary>The save side, for sections that add actions of their own.</summary>
    protected IPortalContentEditor Editor { get; } = editor;

    protected ILogger Logger { get; } = logger;

    protected abstract string SectionView { get; }

    /// <summary>Set by the sections that also have a single line of text.</summary>
    protected virtual PortalTextKey? TextKey => null;

    /// <summary>The order the list shows in. The default is the newest first.</summary>
    protected virtual IEnumerable<TItem> Arrange(IEnumerable<TItem> items) => items;

    /// <summary>Lets a section give its view more to show, such as an uploaded file.</summary>
    protected virtual Task AddViewDataAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("")]
    public async Task<IActionResult> Index(int? edit, CancellationToken cancellationToken)
    {
        var form = new TForm();

        if (edit is { } id)
        {
            var item = await Editor.FindAsync<TItem>(id, cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            form.Load(item);
        }

        return await ShowAsync(form, cancellationToken);
    }

    [HttpPost("")]
    [RequestSizeLimit(ImageRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageRequestLimit)]
    public async Task<IActionResult> Save(TForm form, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(form);

        // The stored item's picture counts, not one named in the form, so a form can't take another item's picture.
        var currentImageId = form.Id == 0 ? null : (await Editor.FindAsync<TItem>(form.Id, cancellationToken) as IHasImage)?.ImageId;
        var imageForm = form as IImageForm;

        if (imageForm is not null)
        {
            imageForm.ImageId = currentImageId;
        }

        if (!ModelState.IsValid)
        {
            return await ShowAsync(form, cancellationToken);
        }

        if (imageForm is not null)
        {
            if (imageForm.Image is { Length: > 0 } file)
            {
                var clock = HttpContext.RequestServices.GetRequiredService<TimeProvider>();
                var (image, problem) = await ImageUpload.ReadAsync(file, clock.GetUtcNow(), cancellationToken);

                if (image is null)
                {
                    ModelState.AddModelError("Form.Image", problem!);
                    return await ShowAsync(form, cancellationToken);
                }

                imageForm.ImageId = await Editor.SaveImageAsync(image, cancellationToken);
            }
            else if (imageForm.RemoveImage)
            {
                imageForm.ImageId = null;
            }
        }

        var item = form.ToItem();
        var saved = await Editor.SaveAsync(item, cancellationToken);

        if (saved)
        {
            Logger.ItemSaved(CurrentUserId, SectionView, item.Id);
        }

        // A picture nothing uses any more goes too: the old one after a replace or remove, or the new one
        // when the item was deleted while the form was open.
        if (imageForm is not null)
        {
            var dropped = saved ? currentImageId : imageForm.ImageId;
            var kept = saved ? imageForm.ImageId : currentImageId;

            if (dropped is { } unused && unused != kept)
            {
                await Editor.DeleteImageAsync(unused, cancellationToken);
            }
        }

        TempData[MasterNotice.Key] = saved ? MasterNotice.Saved : MasterNotice.Missing;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var imageId = (await Editor.FindAsync<TItem>(id, cancellationToken) as IHasImage)?.ImageId;

        await Editor.DeleteAsync<TItem>(id, cancellationToken);

        if (imageId is { } unused)
        {
            await Editor.DeleteImageAsync(unused, cancellationToken);
        }

        Logger.ItemDeleted(CurrentUserId, SectionView, id);
        TempData[MasterNotice.Key] = MasterNotice.Deleted;

        return RedirectToAction(nameof(Index));
    }

    // A form may carry a picture of up to 5 MB, plus room for its other fields.
    private const long ImageRequestLimit = PortalImageRules.MaxBytes + (512 * 1024);

    private async Task<IActionResult> ShowAsync(TForm form, CancellationToken cancellationToken)
    {
        await AddViewDataAsync(cancellationToken);

        var items = await Editor.ListAsync<TItem>(cancellationToken);

        MasterTextForm? text = null;

        if (TextKey is { } key)
        {
            text = new MasterTextForm { Key = key, Value = await Editor.GetTextAsync(key, cancellationToken) };
        }

        return View(
            $"~/Views/Master/{SectionView}.cshtml",
            new MasterSectionViewModel<TItem, TForm>(Arrange(items).ToArray(), form, text));
    }
}
