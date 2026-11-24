using System.Collections;
using System.Diagnostics;
using Domain.Interfaces;
using Domain.Interfaces.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Native.UI.Services;
using Shared;
using Shared.Services;
using Socratic.Platform.Composition.Extensions;
using static SharedKernel.Options.MinioOptions;
using static SharedKernel.ValueObjects.EnvironmentVariables;
using static Shared.Helpers.FileHelper;

#if DEBUG
using MauiDevFlow.Agent;
using MauiDevFlow.Blazor;
#endif

namespace Native.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        AddConfig(builder.Configuration, APPSETTINGS);
#if DEBUG
        AddConfig(builder.Configuration, APPSETTINGS_DEVELOPMENT);
#endif
        builder.Configuration.AddInMemoryCollection(
            Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Where(e => e.Key is string && e.Value is string)
                .Select(e => new KeyValuePair<string, string?>(((string)e.Key).Replace("__", ":"), e.Value as string)));

        MinIOUrl = builder.Configuration[MINIO_URL] ?? MinIOUrl;

        builder.Services.AddCompositionServices(builder.Configuration);
        builder.Services.AddHttpClient(Options.DefaultName, client =>
        {
            if (Uri.TryCreate(builder.Configuration[WEB_UI_URL], UriKind.Absolute, out var baseUri))
                client.BaseAddress = baseUri;
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        builder.Services.AddSingleton<IFormFactor, MauiFormFactor>();
        builder.Services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, Infrastructure.Gateways.Biometrics.ClientFaceIdService>();
        builder.Services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, Infrastructure.Gateways.Support.ClientSupportSessionProvider>();
        builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MauiAuthenticationStateProvider>();
        builder.Services.AddScoped<Infrastructure.Gateways.Identity.IPasskeyService, MauiPasskeyService>();
        builder.Services.AddScoped<Infrastructure.Gateways.Identity.IExternalAuthBrowser, MauiExternalAuthBrowser>();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent();
        builder.AddMauiBlazorDevFlowTools();
#endif

        var app = builder.Build();
        var httpFactory = app.Services.GetRequiredService<IHttpClientFactory>();
        RefreshRemoteConfig(httpFactory, APPSETTINGS);
#if DEBUG
        RefreshRemoteConfig(httpFactory, APPSETTINGS_DEVELOPMENT);
#endif
        return app;
    }

    private static void AddConfig(ConfigurationManager configuration, string fileName)
    {
        var bundledPath = Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(bundledPath))
        {
            configuration.AddJsonFile(bundledPath, optional: true);
        }
#if ANDROID
        else
        {
            try
            {
                using var asset = Android.App.Application.Context.Assets?.Open(fileName);
                if (asset is not null)
                    configuration.AddJsonStream(asset);
            }
            catch { }
        }
#endif
        configuration.AddJsonFile(Path.Combine(FileSystem.AppDataDirectory, fileName), optional: true, reloadOnChange: true);
    }

    private static void RefreshRemoteConfig(IHttpClientFactory httpFactory, string fileName) =>
        _ = Task.Run(async () =>
        {
            try
            {
                using var client = httpFactory.CreateClient();
                if (client.BaseAddress is null) return;

                var json = await client.GetStringAsync(fileName).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json)) return;

                var cachePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                var tempPath = $"{cachePath}.tmp";
                await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);
                File.Move(tempPath, cachePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] Remote {fileName} refresh failed: {ex.Message}");
            }
        });
}
