namespace Itti.Inside.Web.Security;

/// <summary>Adds the browser security headers to every response.</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    // Everything is served from this site: no CDNs, no inline scripts and no inline styles.
    // blob: lets the master page preview a picture the admin has chosen, before it is uploaded.
    private const string ContentSecurityPolicy =
        "default-src 'none'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "form-action 'self'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none'";

    // dotnet watch refreshes the browser over a local WebSocket.
    private static readonly string DevelopmentContentSecurityPolicy =
        ContentSecurityPolicy.Replace("connect-src 'self'", "connect-src 'self' ws://localhost:* wss://localhost:*", StringComparison.Ordinal);

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var headers = context.Response.Headers;

        headers.ContentSecurityPolicy = environment.IsDevelopment() ? DevelopmentContentSecurityPolicy : ContentSecurityPolicy;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        return next(context);
    }
}
