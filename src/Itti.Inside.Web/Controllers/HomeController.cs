using Itti.Inside.Application.Home;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

public sealed class HomeController(IHomeDashboardService dashboard) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await dashboard.GetAsync(cancellationToken));
}
