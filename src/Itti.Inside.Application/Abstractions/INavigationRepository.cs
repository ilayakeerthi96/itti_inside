using Itti.Inside.Domain.Navigation;

namespace Itti.Inside.Application.Abstractions;

/// <summary>The header menu, the content pages behind it and the Applications tiles.</summary>
public interface INavigationRepository
{
    Task<IReadOnlyList<NavigationItem>> GetMenuAsync(CancellationToken cancellationToken);

    /// <summary>Finds a page by its URL path, for example <c>/hr/leave-policy</c>.</summary>
    Task<PortalPage?> FindPageAsync(string path, CancellationToken cancellationToken);

    Task<IReadOnlyList<PortalApplication>> GetApplicationsAsync(CancellationToken cancellationToken);
}
