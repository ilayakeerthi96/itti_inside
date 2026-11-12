using Itti.Inside.Application.Abstractions;
using Itti.Inside.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>
/// The full pages behind the landing page cards. A card shows the newest few items; its page shows
/// every one. They open without signing in, like the landing page.
/// </summary>
[AllowAnonymous]
public sealed class DetailsController(IPortalContentRepository content) : Controller
{
    [HttpGet("ceo-desk")]
    public async Task<IActionResult> CeoDesk(CancellationToken cancellationToken) =>
        View(await content.GetCeoDeskPostsAsync(cancellationToken));

    [HttpGet("customer-feedback")]
    public async Task<IActionResult> CustomerFeedback(CancellationToken cancellationToken) =>
        View(await content.GetAllCustomerFeedbackAsync(cancellationToken));

    [HttpGet("rockstar-awards")]
    public async Task<IActionResult> RockstarAwards(CancellationToken cancellationToken) =>
        View(await content.GetAllRockstarAwardsAsync(cancellationToken));

    [HttpGet("birthdays")]
    public async Task<IActionResult> Birthdays(CancellationToken cancellationToken) =>
        View(await content.GetBirthdaysAsync(cancellationToken));

    [HttpGet("wedding-bells")]
    public async Task<IActionResult> WeddingBells(CancellationToken cancellationToken) =>
        View("Celebration", new CelebrationPageViewModel("Wedding Bells", "wedding", await content.GetWeddingBellsAsync(cancellationToken)));

    [HttpGet("new-born")]
    public async Task<IActionResult> NewBorn(CancellationToken cancellationToken) =>
        View("Celebration", new CelebrationPageViewModel("New Born", "newborn", await content.GetNewBornsAsync(cancellationToken)));
}
