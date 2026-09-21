using Microsoft.Extensions.DependencyInjection;
using Markdown.Web;

namespace Markdown.Web
{
    public static class MarkdownServiceCollectionExtensions
    {
        public static IServiceCollection AddMarkdownServices(this IServiceCollection services)
        {
            return services.AddScoped<IStaticAssetService, ServerStaticAssetService>();
        }
    }
}
