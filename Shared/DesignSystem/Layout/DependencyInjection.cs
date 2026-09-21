using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Abstractions;
using Shared.Services;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Core Client Infrastructure
        services.TryAddScoped<ISettingsManager, SettingsManager>();
        services.TryAddScoped<IFormFactor, WebFormFactor>();

        // Standalone Mocks for Sandbox hosts (overridable by production apps)
        services.AddAuthorizationCore();
        services.TryAddScoped<AuthenticationStateProvider, StandaloneAuthenticationStateProvider>();
        services.TryAddScoped<IServiceClientFactory, StandaloneServiceClientFactory>();

        // Feature Services (Identity, Biometrics, AI, Realtime)
        services.TryAddScoped<IPasskeyService, PasskeyService>();
        services.TryAddScoped<Domain.Interfaces.Biometrics.IFaceIdService, ClientFaceIdService>();
        services.TryAddScoped<ClientFaceIdService>();
        services.TryAddScoped<Domain.Interfaces.Pages.IDetectorService, RemoteDetectorService>();
        services.TryAddScoped<RemoteDetectorService>();
        services.TryAddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, ClientSupportSessionProvider>();
        services.TryAddScoped<ClientSupportSessionProvider>();
        services.TryAddScoped<Smart.Web.Services.SmartToastService>();

        return services;
    }
}
