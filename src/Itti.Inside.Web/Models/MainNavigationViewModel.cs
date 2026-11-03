using Itti.Inside.Domain.Navigation;

namespace Itti.Inside.Web.Models;

public sealed record MainNavigationViewModel(IReadOnlyList<NavigationItem> Items, string CurrentPath)
{
    public bool IsActive(NavigationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.HasChildren)
        {
            return item.Children.Any(IsActive);
        }

        return item.Path == "/"
            ? CurrentPath == "/"
            : item.Path is not null && CurrentPath.StartsWith(item.Path, StringComparison.OrdinalIgnoreCase);
    }
}
