using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Domain.Interfaces;
using Domain.Interfaces.Pages;
using Shared;
using Shared.Services;
using Smart.Web;
#if FEATURE_MAP
using Map;
#endif
#if FEATURE_MARKDOWN
using Markdown.Web;
#endif
#if FEATURE_FFMPEG
using FFmpegProcessor;
#endif
#if FEATURE_BIOMETRICS
using Biometrics.Processor;
#endif

namespace Apps.Composition.Extensions
{
    /// <summary>
    /// Centralized service registration and feature module discovery for all platform hosts (Web, MAUI, Desktop).
    /// </summary>
    public static class CompositionServiceCollectionExtensions
    {
        private static bool _featuresRegistered;
        private static readonly object _lock = new();

        /// <summary>
        /// Registers all 13 feature modules in FeatureRegistry.
        /// </summary>
        public static void RegisterFeatureModules()
        {
            if (_featuresRegistered) return;
            lock (_lock)
            {
                if (_featuresRegistered) return;

#if FEATURE_POS
                FeatureRegistry.Register(new Socratic.POS.POSFeatureModule());
#endif
#if FEATURE_KIOSK
                FeatureRegistry.Register(new Socratic.Kiosk.KioskFeatureModule());
#endif
#if FEATURE_COMMERCE
                FeatureRegistry.Register(new Socratic.Commerce.CommerceFeatureModule());
#endif
#if FEATURE_VISION
                FeatureRegistry.Register(new Socratic.Vision.VisionFeatureModule());
#endif
#if FEATURE_CHAT
                FeatureRegistry.Register(new Chat.ChatFeatureModule());
#endif
#if FEATURE_LANDING
                FeatureRegistry.Register(new Socratic.Landing.LandingFeatureModule());
#endif
#if FEATURE_CHECKOUT
                FeatureRegistry.Register(new Socratic.Checkout.CheckoutFeatureModule());
#endif
#if FEATURE_ORDERS
                FeatureRegistry.Register(new Socratic.Orders.OrdersFeatureModule());
#endif
#if FEATURE_ORGANIZATION
                FeatureRegistry.Register(new Socratic.Organization.OrganizationFeatureModule());
#endif
#if FEATURE_IDENTITY
                FeatureRegistry.Register(new Socratic.Identity.IdentityFeatureModule());
#endif
#if FEATURE_MAP
                FeatureRegistry.Register(new Map.MapFeatureModule());
#endif
#if FEATURE_QRDESIGNER
                FeatureRegistry.Register(new QrDesigner.QrDesignerFeatureModule());
#endif
#if FEATURE_SEATDESIGNER
                FeatureRegistry.Register(new SeatDesigner.SeatDesignerFeatureModule());
#endif

                _featuresRegistered = true;
            }
        }

        /// <summary>
        /// Adds all universal composition services, design system components, gRPC clients, and registered features.
        /// </summary>
        public static IServiceCollection AddCompositionServices(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // 1. Register Feature Modules in the dynamic registry
            RegisterFeatureModules();

            // 2. Register UI and Design System services
            services.AddSmartWebServices();
#if FEATURE_MAP
            services.AddMapServices();
#endif
#if FEATURE_MARKDOWN
            services.AddMarkdownServices();
#endif
#if FEATURE_FFMPEG
            services.AddFFmpegProcessorServices();
#endif
#if FEATURE_BIOMETRICS
            services.AddBiometricProcessorServices();
#endif

            // 3. Register Core & Shell services
#if FEATURE_CHAT
            services.AddScoped<Chat.Abstractions.IChatService, Chat.Services.SignalRChatService>();
#endif
            services.AddScoped<IDetectorService, RemoteDetectorService>();
            services.AddScoped<ISettingsManager, SettingsManager>();
            services.AddScoped<IPasskeyService, PasskeyService>();
            services.AddLocalization().AddScoped<IStringLocalizer, StringLocalizer>();

            // 4. gRPC Clients & Auth
            if (configuration != null)
            {
                services.AddGrpcClients(configuration);
            }

            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();

            // 5. Feature module service extensions
            if (configuration != null)
            {
                services.AddRegisteredFeatureServices(configuration);
            }

            return services;
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="AddCompositionServices"/>.
        /// </summary>
        [Obsolete("Use AddCompositionServices instead.")]
        public static IServiceCollection AddSharedAppServices(this IServiceCollection services, IConfiguration? configuration = null)
            => AddCompositionServices(services, configuration);
    }
}

namespace Apps.Shared.Extensions
{
    /// <summary>
    /// Backward-compatibility wrapper for legacy references.
    /// </summary>
    public static class SharedAppServiceCollectionExtensions
    {
        /// <summary>
        /// Backward-compatible alias for <see cref="Apps.Composition.Extensions.CompositionServiceCollectionExtensions.AddCompositionServices"/>.
        /// </summary>
        [Obsolete("Use AddCompositionServices instead.")]
        public static IServiceCollection AddSharedAppServices(this IServiceCollection services, IConfiguration? configuration = null)
            => Apps.Composition.Extensions.CompositionServiceCollectionExtensions.AddCompositionServices(services, configuration);
    }
}
