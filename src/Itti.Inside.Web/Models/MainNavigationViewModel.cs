using Itti.Inside.Domain.Navigation;

namespace Itti.Inside.Web.Models;

/// <param name="Items">The portal menu.</param>
/// <param name="CurrentPath">The page being shown, without the path base.</param>
/// <param name="IsMasterArea">True on the master pages, which show the admin header instead of the menu.</param>
public sealed record MainNavigationViewModel(IReadOnlyList<NavigationItem> Items, string CurrentPath, bool IsMasterArea)
{
    public bool IsActive(NavigationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.HasChildren)
        {
            return item.Children.Any(IsActive);
        }

        if (item.IsExternal)
        {
            return false;
        }

        return item.Path == "/"
            ? CurrentPath == "/"
            : item.Path is not null && CurrentPath.StartsWith(item.Path, StringComparison.OrdinalIgnoreCase);
    }
}
