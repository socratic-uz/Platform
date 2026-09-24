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
using Apps.Composition.Extensions;
using Shared;
using Shared.Services;
using Domain.Interfaces;
using Domain.Interfaces.Pages;

using static SharedKernel.Options.MinioOptions;
using static Shared.Helpers.FileHelper;

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

            // 1. Загрузка конфигураций из Embedded Resource (гарантированно работает на Android, iOS, Windows, MacCatalyst)
            var assembly = typeof(MauiProgram).Assembly;
            try
            {
                using var baseConfigStream = assembly.GetManifestResourceStream("appsettings.json");
                if (baseConfigStream != null)
                {
                    builder.Configuration.AddJsonStream(baseConfigStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] Embedded appsettings.json load error: {ex.Message}");
            }

#if DEBUG
            try
            {
                using var devConfigStream = assembly.GetManifestResourceStream("appsettings.Development.json");
                if (devConfigStream != null)
                {
                    builder.Configuration.AddJsonStream(devConfigStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] Embedded appsettings.Development.json load error: {ex.Message}");
            }
#endif

            // 2. Локальное переопределение из физического файла на диске (для Windows Desktop / локальной отладки)
            try
            {
                builder.Configuration.AddJsonFile(EnvironmentVariables.APPSETTINGS, optional: true, reloadOnChange: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] File appsettings.json load info: {ex.Message}");
            }

#if DEBUG
            try
            {
                builder.Configuration.AddJsonFile(EnvironmentVariables.APPSETTINGS_DEVELOPMENT, optional: true, reloadOnChange: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] File appsettings.Development.json load info: {ex.Message}");
            }
#endif

            MinIOUrl = builder.Configuration[MINIO_URL] ?? MinIOUrl;

            // 1. Единое подключение всех возможностей Composition (Shell, 13 фич, дизайн-система, gRPC, аппаратные интерфейсы)
            builder.Services.AddCompositionServices(builder.Configuration);

            // 2. Специфичные платформенные адаптеры для MAUI
            builder.Services.AddSingleton<IFormFactor, MauiFormFactor>();
            builder.Services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, ClientFaceIdService>();
            builder.Services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, ClientSupportSessionProvider>();
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MauiAuthenticationStateProvider>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
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
    }
}
