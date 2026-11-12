using Itti.Inside.Application.Home;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itti.Inside.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // The landing page decides what is new this month from the server's clock. Tests can replace it.
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IHomeDashboardService, HomeDashboardService>();

        return services;
    }
}
