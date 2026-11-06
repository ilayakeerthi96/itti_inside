using Itti.Inside.Application.Abstractions;
using Itti.Inside.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.ViewComponents;

public sealed class MainNavigationViewComponent(INavigationRepository navigation) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await navigation.GetMenuAsync(HttpContext.RequestAborted);

        var path = HttpContext.Request.Path;

        return View(new MainNavigationViewModel(items, path, path.StartsWithSegments("/master", StringComparison.OrdinalIgnoreCase)));
    }
}
