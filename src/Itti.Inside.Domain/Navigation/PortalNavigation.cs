namespace Itti.Inside.Domain.Navigation;

/// <summary>A header menu entry. An entry with <see cref="Children"/> renders as a dropdown.</summary>
public sealed record NavigationItem(string Title, string? Path, IReadOnlyList<NavigationItem> Children)
{
    public bool HasChildren => Children.Count > 0;
}

/// <summary>A content page reachable by URL.</summary>
public sealed record PortalPage(string Path, string Title, string? Section, string Summary);

/// <summary>A tile on the Applications page.</summary>
public sealed record PortalApplication(string Name, string Description, string Icon, string Path);
