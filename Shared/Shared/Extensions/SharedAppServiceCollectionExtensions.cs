using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Domain.Interfaces;
using Domain.Interfaces.Pages;
using Shared;
using Shared.Services;
using Smart.Web;
using Map;
using Markdown.Web;
using FFmpegProcessor;
using Biometrics.Processor;

namespace Apps.Shared.Extensions;

/// <summary>
/// Centralized service registration and feature module discovery for all platform hosts (Web, MAUI, Desktop).
/// </summary>
public static class SharedAppServiceCollectionExtensions
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

            FeatureRegistry.Register(new Socratic.POS.POSFeatureModule());
            FeatureRegistry.Register(new Socratic.Kiosk.KioskFeatureModule());
            FeatureRegistry.Register(new Socratic.Commerce.CommerceFeatureModule());
            FeatureRegistry.Register(new Socratic.Vision.VisionFeatureModule());
            FeatureRegistry.Register(new Chat.ChatFeatureModule());
            FeatureRegistry.Register(new Socratic.Landing.LandingFeatureModule());
            FeatureRegistry.Register(new Socratic.Checkout.CheckoutFeatureModule());
            FeatureRegistry.Register(new Socratic.Orders.OrdersFeatureModule());
            FeatureRegistry.Register(new Socratic.Organization.OrganizationFeatureModule());
            FeatureRegistry.Register(new Socratic.Identity.IdentityFeatureModule());
            FeatureRegistry.Register(new Map.MapFeatureModule());
            FeatureRegistry.Register(new QrDesigner.QrDesignerFeatureModule());
            FeatureRegistry.Register(new SeatDesigner.SeatDesignerFeatureModule());

            _featuresRegistered = true;
        }
    }

    /// <summary>
    /// Adds all universal shared application services, design system components, gRPC clients, and registered features.
    /// </summary>
    public static IServiceCollection AddSharedAppServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // 1. Register Feature Modules in the dynamic registry
        RegisterFeatureModules();

        // 2. Register UI and Design System services
        services.AddSmartWebServices();
        services.AddMapServices();
        services.AddMarkdownServices();
        services.AddFFmpegProcessorServices();
        services.AddBiometricProcessorServices();

        // 3. Register Core & Shell services
        services.AddScoped<Chat.Abstractions.IChatService, Chat.Services.SignalRChatService>();
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
}
