namespace Itti.Inside.Domain.Navigation;

/// <summary>
/// A header menu entry. An entry with <see cref="Children"/> renders as a dropdown. The path is either
/// a page on this site, such as <c>/hr/travel-desk</c>, or a full address on another site.
/// </summary>
public sealed record NavigationItem(string Title, string? Path, IReadOnlyList<NavigationItem> Children)
{
    public bool HasChildren => Children.Count > 0;

    /// <summary>True for a link to another site, such as a page of the current intranet.</summary>
    public bool IsExternal =>
        Uri.TryCreate(Path, UriKind.Absolute, out var address)
        && (address.Scheme == Uri.UriSchemeHttps || address.Scheme == Uri.UriSchemeHttp);
}

/// <summary>A content page reachable by URL.</summary>
public sealed record PortalPage(string Path, string Title, string? Section, string Summary);

/// <summary>A tile on the Applications page.</summary>
public sealed record PortalApplication(string Name, string Description, string Icon, string Path);
