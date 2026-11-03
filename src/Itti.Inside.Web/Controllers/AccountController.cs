using System.Security.Claims;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Application.Identity;
using Itti.Inside.Web.Models;
using Itti.Inside.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Itti.Inside.Web.Controllers;

/// <summary>
/// One sign-in page for everyone. The account decides where it leads: an admin account opens a
/// session that holds only the master page, and an employee account opens the portal.
/// </summary>
[Route("account")]
public sealed partial class AccountController(
    ICredentialValidator credentialValidator,
    ILogger<AccountController> logger) : Controller
{
    private const string MasterHome = "~/master";
    private const string PortalHome = "~/";

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.IsInRole(PortalRoles.Admin))
        {
            return LocalRedirect(Url.Content(MasterHome));
        }

        if (User.IsInRole(PortalRoles.Employee))
        {
            return LocalRedirect(SafeReturnUrl(returnUrl));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.SignIn)]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await credentialValidator.ValidateAsync(model.UserId, model.Password, cancellationToken);

        // An account with neither role is treated like a wrong password.
        var role = user?.Roles.Contains(PortalRoles.Admin) == true ? PortalRoles.Admin
            : user?.Roles.Contains(PortalRoles.Employee) == true ? PortalRoles.Employee
            : null;

        if (user is null || role is null)
        {
            // The user ID is left out on purpose: people sometimes type their password into that box.
            LogSignInFailed(logger, HttpContext.Connection.RemoteIpAddress?.ToString());

            ModelState.AddModelError(string.Empty, "The User ID or Password you entered is incorrect.");
            return View(model);
        }

        await SignInAsync(user, role);
        LogSignInSucceeded(logger, user.UserId, role);

        return LocalRedirect(role == PortalRoles.Admin ? AdminReturnUrl(model.ReturnUrl) : SafeReturnUrl(model.ReturnUrl));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    // The session gets one role: admin if the account has it, otherwise employee.
    private Task SignInAsync(AuthenticatedUser user, string role)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, role),
        ];

        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }

    // Blocks open redirects: only paths on this site are honoured.
    private string SafeReturnUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content(PortalHome);

    // An admin session holds only the master page, so any other return address leads there.
    private string AdminReturnUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/master", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : Url.Content(MasterHome);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed sign-in attempt from {ClientAddress}.")]
    private static partial void LogSignInFailed(ILogger logger, string? clientAddress);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} signed in as {Role}.")]
    private static partial void LogSignInSucceeded(ILogger logger, string userId, string role);
}
