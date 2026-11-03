using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Navigation;
using Itti.Inside.Infrastructure.Seed;

namespace Itti.Inside.Infrastructure.Content;

/// <summary>Serves the header menu and dummy pages from the seed file.</summary>
public sealed class SeedNavigationRepository(PortalSeedData seed) : INavigationRepository
{
    public Task<IReadOnlyList<NavigationItem>> GetMenuAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<NavigationItem> menu = seed.Menu.Select(item => item.ToNavigationItem()).ToArray();

        return Task.FromResult(menu);
    }

    public Task<PortalPage?> FindPageAsync(string path, CancellationToken cancellationToken)
    {
        var page = seed.Pages.FirstOrDefault(candidate =>
            string.Equals(candidate.Path, path, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(page);
    }

    public Task<IReadOnlyList<PortalApplication>> GetApplicationsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(seed.Applications);
}
