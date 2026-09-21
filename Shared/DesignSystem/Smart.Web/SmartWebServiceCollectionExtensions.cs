using Microsoft.Extensions.DependencyInjection;
using Smart.Web.Services;

namespace Smart.Web;

public static class SmartWebServiceCollectionExtensions
{
    public static IServiceCollection AddSmartWebServices(this IServiceCollection services)
    {
        services.AddScoped<SmartToastService>();
        return services;
    }
}
