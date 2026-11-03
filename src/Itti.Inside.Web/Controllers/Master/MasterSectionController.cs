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
    protected abstract string SectionView { get; }

    /// <summary>Set by the sections that also have a single line of text.</summary>
    protected virtual PortalTextKey? TextKey => null;

    /// <summary>The order the list shows in. The default is the newest first.</summary>
    protected virtual IEnumerable<TItem> Arrange(IEnumerable<TItem> items) => items;

    [HttpGet("")]
    public async Task<IActionResult> Index(int? edit, CancellationToken cancellationToken)
    {
        var form = new TForm();

        if (edit is { } id)
        {
            var item = await editor.FindAsync<TItem>(id, cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            form.Load(item);
        }

        return await ShowAsync(form, cancellationToken);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(TForm form, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(form);

        if (!ModelState.IsValid)
        {
            return await ShowAsync(form, cancellationToken);
        }

        var item = form.ToItem();
        var saved = await editor.SaveAsync(item, cancellationToken);

        if (saved)
        {
            logger.ItemSaved(CurrentUserId, SectionView, item.Id);
        }

        TempData[MasterNotice.Key] = saved ? MasterNotice.Saved : MasterNotice.Missing;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await editor.DeleteAsync<TItem>(id, cancellationToken);

        logger.ItemDeleted(CurrentUserId, SectionView, id);
        TempData[MasterNotice.Key] = MasterNotice.Deleted;

        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<IActionResult> ShowAsync(TForm form, CancellationToken cancellationToken)
    {
        var items = await editor.ListAsync<TItem>(cancellationToken);

        MasterTextForm? text = null;

        if (TextKey is { } key)
        {
            text = new MasterTextForm { Key = key, Value = await editor.GetTextAsync(key, cancellationToken) };
        }

        return View(
            $"~/Views/Master/{SectionView}.cshtml",
            new MasterSectionViewModel<TItem, TForm>(Arrange(items).ToArray(), form, text));
    }
}
