using Itti.Inside.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>
/// Friendly error pages. Accepts every HTTP verb because the error pipeline
/// re-runs the failed request, which may have been a POST.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("error")]
public sealed class ErrorController : Controller
{
    [Route("{statusCode:int?}")]
    public IActionResult Status(int statusCode = StatusCodes.Status500InternalServerError)
    {
        Response.StatusCode = statusCode;

        return View(ErrorViewModel.ForStatus(statusCode));
    }
}
