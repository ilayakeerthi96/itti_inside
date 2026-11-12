using Itti.Inside.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>The CEO's Desk and Rockstar Award pictures.</summary>
[AllowAnonymous]
public sealed class MediaController(IPortalContentRepository content) : Controller
{
    /// <summary>
    /// A new picture always gets a new Id, so the browser may keep each one for a year. Overrides the
    /// site-wide no-store rule, which is meant for pages.
    /// </summary>
    [HttpGet("media/{id:int}")]
    [ResponseCache(Duration = 31_536_000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Image(int id, CancellationToken cancellationToken) =>
        await content.GetImageAsync(id, cancellationToken) is { } image
            ? File(image.Content, image.ContentType)
            : NotFound();
}
