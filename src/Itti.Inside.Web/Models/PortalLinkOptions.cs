namespace Itti.Inside.Web.Models;

/// <summary>
/// Pages on other sites that the landing page links to, bound from the <c>Links</c> section. They live in
/// configuration, so the server can point them elsewhere without a new build.
/// </summary>
public sealed class PortalLinkOptions
{
    public const string SectionName = "Links";

    /// <summary>
    /// Where the CEO's Desk "More" button leads, as a full http:// or https:// address. When empty, it
    /// leads to this site's own CEO's Desk page.
    /// </summary>
    public string CeoDesk { get; set; } = string.Empty;

    public static bool IsWebAddress(string address) =>
        Uri.TryCreate(address, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
