using Itti.Inside.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>The dummy pages behind the header menu. Give a page its own controller as it gets built.</summary>
public sealed class PagesController(INavigationRepository navigation) : Controller
{
    [HttpGet("applications")]
    public async Task<IActionResult> Applications(CancellationToken cancellationToken) =>
        View(await navigation.GetApplicationsAsync(cancellationToken));

    [HttpGet("{**path}", Order = int.MaxValue)]
    public async Task<IActionResult> Show(string path, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);

        var page = await navigation.FindPageAsync($"/{path.TrimEnd('/')}", cancellationToken);

        return page is null ? NotFound() : View(page);
    }
}
