using Itti.Inside.Web.Security;

namespace Itti.Inside.Web.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UsePortalPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStatusCodePagesWithReExecute("/error/{0}");

        // Sits after the error handlers so re-executed error pages get the headers too.
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health").AllowAnonymous();

        // The sign-in page needs its styles before anyone is signed in.
        app.MapStaticAssets().AllowAnonymous();

        app.MapControllers().WithStaticAssets();

        return app;
    }
}
