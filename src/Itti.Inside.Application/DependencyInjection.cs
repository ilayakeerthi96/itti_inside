using Itti.Inside.Application.Home;
using Microsoft.Extensions.DependencyInjection;

namespace Itti.Inside.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHomeDashboardService, HomeDashboardService>();

        return services;
    }
}
