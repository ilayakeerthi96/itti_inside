using Itti.Inside.Application.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>The landing page. It opens straight away, without signing in.</summary>
[AllowAnonymous]
public sealed class HomeController(IHomeDashboardService dashboard) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await dashboard.GetAsync(cancellationToken));
}
