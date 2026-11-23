using Biometrics.Processor;
using FFmpegProcessor;
using Map;
using Markdown.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SharedKernel.ValueObjects;
using System.Diagnostics;
using Native.UI.Services;
using Socratic.Platform.Composition.Extensions;
using Shared;
using Shared.Services;
using Domain.Interfaces;
using Domain.Interfaces.Pages;

using static SharedKernel.Options.MinioOptions;
using static Shared.Helpers.FileHelper;

#if DEBUG
using MauiDevFlow.Agent;
using MauiDevFlow.Blazor;
#endif

namespace Native.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 1. Удалённая конфигурация (https://socratic.uz/appsettings.json и appsettings.Development.json) с локальным кэшем на устройстве
            LoadRemoteJson(builder.Configuration, EnvironmentVariables.APPSETTINGS);
#if DEBUG
            LoadRemoteJson(builder.Configuration, EnvironmentVariables.APPSETTINGS_DEVELOPMENT);
#endif

            // 2. Переменные окружения (Aspire и локальный запуск переопределяют удалённый конфиг)
            var envVars = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string key && entry.Value is string value)
                {
                    envVars[key.Replace("__", ":")] = value;
                }
            }
            if (envVars.Count > 0)
            {
                builder.Configuration.AddInMemoryCollection(envVars);
            }

            MinIOUrl = builder.Configuration[MINIO_URL] ?? MinIOUrl;

            // 1. Единое подключение всех возможностей Composition (Layout, 13 фич, дизайн-система, gRPC, аппаратные интерфейсы)
            builder.Services.AddCompositionServices(builder.Configuration);

            // 2. Специфичные платформенные адаптеры для MAUI
            builder.Services.AddSingleton<IFormFactor, MauiFormFactor>();
            builder.Services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, Infrastructure.Gateways.Biometrics.ClientFaceIdService>();
            builder.Services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, Infrastructure.Gateways.Support.ClientSupportSessionProvider>();
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MauiAuthenticationStateProvider>();
            builder.Services.AddScoped<Infrastructure.Gateways.Identity.IPasskeyService, Services.MauiPasskeyService>();
            builder.Services.AddScoped<Infrastructure.Gateways.Identity.IExternalAuthBrowser, Services.MauiExternalAuthBrowser>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
            builder.AddMauiDevFlowAgent();
            builder.AddMauiBlazorDevFlowTools();
#endif

            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Socratic", "Logs");
                Directory.CreateDirectory(logDir);
                var configDiagnostic = $"[Socratic MAUI Diagnostic Log - {DateTime.Now}]\n" +
                                       $"Identifying URL: {builder.Configuration[SharedKernel.ValueObjects.EnvironmentVariables.IDENTIFYING_SERVICE_URL] ?? "http://localhost:5001 (default)"}\n" +
                                       $"Shopping URL:    {builder.Configuration[SharedKernel.ValueObjects.EnvironmentVariables.SHOPPING_SERVICE_URL] ?? "http://localhost:5002 (default)"}\n" +
                                       $"Ordering URL:    {builder.Configuration[SharedKernel.ValueObjects.EnvironmentVariables.ORDERING_SERVICE_URL] ?? "http://localhost:5003 (default)"}\n" +
                                       $"Paying URL:      {builder.Configuration[SharedKernel.ValueObjects.EnvironmentVariables.PAYING_SERVICE_URL] ?? "http://localhost:5004 (default)"}\n";
                File.WriteAllText(Path.Combine(logDir, "maui_config.log"), configDiagnostic);
                Console.WriteLine(configDiagnostic);
                System.Diagnostics.Debug.WriteLine(configDiagnostic);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram Diagnostic Log Error]: {ex.Message}");
            }

            try
            {
                return builder.Build();
            }
            catch (Exception ex)
            {
                try
                {
                    var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Socratic", "Logs");
                    Directory.CreateDirectory(logDir);
                    File.WriteAllText(Path.Combine(logDir, "maui_crash.txt"), ex.ToString());
                }
                catch (Exception writeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[MauiProgram Crash Log Write Error]: {writeEx.Message}");
                }
                throw;
            }
        }

        private const string RemoteConfigBaseUrl = "https://socratic.uz/";

        private static void LoadRemoteJson(ConfigurationManager configuration, string fileName)
        {
            var url = $"{RemoteConfigBaseUrl}{fileName}";
            var cachePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(cachePath))
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                    var json = http.GetStringAsync(url).GetAwaiter().GetResult();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        File.WriteAllText(cachePath, json);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Config] Initial remote {fileName} fetch error: {ex.Message}");
                }
            }
            else
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                        var json = await http.GetStringAsync(url).ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            await File.WriteAllTextAsync(cachePath, json).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Config] Background remote {fileName} refresh error: {ex.Message}");
                    }
                });
            }

            configuration.AddJsonFile(cachePath, optional: true, reloadOnChange: true);
        }
    }
}
