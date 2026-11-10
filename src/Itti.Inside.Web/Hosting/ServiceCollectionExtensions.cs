using System.Net;
using System.Threading.RateLimiting;
using Itti.Inside.Application.Identity;
using Itti.Inside.Web.Models;
using Itti.Inside.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Itti.Inside.Web.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortalWeb(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var cookieSecurity = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                // Portal pages show employee data, so browsers and proxies must not keep a copy.
                options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
            })
            .AddCookieTempDataProvider(options => options.Cookie.SecurePolicy = cookieSecurity);

        services.AddPortalAuthentication(cookieSecurity);
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = ".Itti.Inside.Antiforgery";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = cookieSecurity;
        });

        services.AddSignInRateLimit(configuration);
        services.AddReverseProxySupport(configuration);
        services.AddSharedDataProtection(configuration);

        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));
        services.AddHealthChecks();

        services.AddOptions<PortalLinkOptions>()
            .Bind(configuration.GetSection(PortalLinkOptions.SectionName))
            .Validate(
                links => links.CeoDesk.Length == 0 || PortalLinkOptions.IsWebAddress(links.CeoDesk),
                "Links:CeoDesk must be a full http:// or https:// address, or empty.")
            .ValidateOnStart();

        return services;
    }

    private static void AddPortalAuthentication(this IServiceCollection services, CookieSecurePolicy cookieSecurity)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".Itti.Inside.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = cookieSecurity;
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/error/403";
                options.SlidingExpiration = true;
            });

        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<PortalAuthenticationOptions>>((cookie, portal) =>
                cookie.ExpireTimeSpan = TimeSpan.FromMinutes(portal.Value.SessionMinutes));

        // The portal pages open without signing in: everyone on the intranet sees them, so their
        // controllers say [AllowAnonymous]. Anything that doesn't say so, the master pages included,
        // needs an admin session. A new page therefore starts out protected until it is opened up.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireRole(PortalRoles.Admin).Build())
            .AddPolicy(PortalPolicies.ManageContent, policy => policy.RequireRole(PortalRoles.Admin));
    }

    private static void AddSignInRateLimit(this IServiceCollection services, IConfiguration configuration)
    {
        var attemptsPerMinute = configuration.GetValue("RateLimiting:SignInAttemptsPerMinute", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.SignIn, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = attemptsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                    }));
        });
    }

    private static void AddReverseProxySupport(this IServiceCollection services, IConfiguration configuration)
    {
        var knownProxies = configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (var proxy in knownProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });
    }

    // Sign-in cookies are encrypted with these keys. Sharing a folder between servers
    // keeps people signed in across a web farm and across redeployments.
    private static void AddSharedDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var dataProtection = services.AddDataProtection().SetApplicationName("Itti.Inside");
        var keysPath = configuration["DataProtection:KeysPath"];

        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }
    }
}
