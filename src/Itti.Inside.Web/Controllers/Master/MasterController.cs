using System.Security.Claims;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Web.Models.Master;
using Itti.Inside.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers.Master;

/// <summary>The master page entry point, plus the single text values used by several sections.</summary>
[Authorize(Policy = PortalPolicies.ManageContent)]
[Route("master")]
public sealed class MasterController(IPortalContentEditor editor, ILogger<MasterController> logger) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => LocalRedirect(SectionUrl(MasterSections.All[0]));

    [HttpGet("ceo-desk")]
    public async Task<IActionResult> CeoDesk(CancellationToken cancellationToken) =>
        View(new MasterTextForm
        {
            Key = PortalTextKey.CeoMessage,
            Value = await editor.GetTextAsync(PortalTextKey.CeoMessage, cancellationToken),
        });

    [HttpPost("text")]
    public async Task<IActionResult> SaveText(MasterTextForm form, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(form);

        if (ModelState.IsValid)
        {
            await editor.SetTextAsync(form.Key, form.Value.Trim(), cancellationToken);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            logger.TextSaved(userId, form.Key);
        }

        TempData[MasterNotice.Key] = ModelState.IsValid ? MasterNotice.Saved : MasterNotice.TextNotSaved;

        return LocalRedirect(SectionUrl(MasterSections.ForText(form.Key)));
    }

    private string SectionUrl(MasterSection section) => Url.Content($"~/master/{section.Path}");
}
