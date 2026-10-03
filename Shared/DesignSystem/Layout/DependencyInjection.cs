using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Abstractions;
using Shared.Services;

namespace Socratic.Platform.Layout.Extensions
{
    public static class LayoutServiceCollectionExtensions
    {
        /// <summary>
        /// Registers core Layout and UI infrastructure services.
        /// </summary>
        public static IServiceCollection AddLayoutServices(this IServiceCollection services)
        {
            // Core Client Infrastructure
            services.TryAddScoped<ISettingsManager, SettingsManager>();
            services.TryAddScoped<IFormFactor, WebFormFactor>();

            // Standalone Mocks for Sandbox hosts (overridable by production apps)
            services.AddAuthorizationCore();
            services.TryAddScoped<AuthenticationStateProvider, StandaloneAuthenticationStateProvider>();
            services.TryAddScoped<IServiceClientFactory, StandaloneServiceClientFactory>();

            return services;
        }
    }
}

namespace Shared
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Backward-compatible alias for <see cref="Socratic.Platform.Layout.Extensions.LayoutServiceCollectionExtensions.AddLayoutServices"/>.
        /// </summary>
        [Obsolete("Use Socratic.Platform.Layout.Extensions.LayoutServiceCollectionExtensions.AddLayoutServices instead.")]
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
            => Socratic.Platform.Layout.Extensions.LayoutServiceCollectionExtensions.AddLayoutServices(services);
    }
}
