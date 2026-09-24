using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Gateways.Vision;

public static class HubUrlResolver
{
    private static void LogDebug(string message)
    {
        var msg = $"[HubUrlResolver {DateTime.UtcNow:HH:mm:ss.fff}] {message}";
        Console.WriteLine(msg);
        System.Diagnostics.Debug.WriteLine(msg);
    }

    public static string ResolveHubUrl(NavigationManager? navManager, IConfiguration? configuration, string relativePath)
    {
        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;

        // 1. If running in a standard web browser context with a valid HTTP/HTTPS base URI
        if (navManager != null)
        {
            var baseUri = navManager.BaseUri;
            LogDebug($"Checking navManager.BaseUri: '{baseUri}'");
            if (!string.IsNullOrWhiteSpace(baseUri) &&
                (baseUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 baseUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                !baseUri.Contains("0.0.1") &&
                !baseUri.StartsWith("app://", StringComparison.OrdinalIgnoreCase))
            {
                var result = navManager.ToAbsoluteUri(path).ToString();
                LogDebug($"Resolved from navManager: '{result}'");
                return result;
            }
        }

        // 2. If configured via Aspire Service Discovery, Environment Variables, or appsettings
        if (configuration != null)
        {
            var configuredUrl = configuration["services__webui__https__0"]
                                ?? configuration["services__webui__http__0"]
                                ?? configuration["services__webui__default__0"]
                                ?? configuration["ServerUrl"]
                                ?? configuration["RemoteDetectorHubUrl"]
                                ?? configuration["ChatHubUrl"];

            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                var result = $"{configuredUrl.TrimEnd('/')}{path}";
                LogDebug($"Resolved from configuration: '{result}'");
                return result;
            }
        }

        // 3. Fallback for MAUI/Desktop/Android Emulator local testing
        var fallbackHost = OperatingSystem.IsAndroid() ? "http://10.0.2.2:5000" : "http://localhost:5000";
        var fallback = $"{fallbackHost}{path}";
        LogDebug($"Resolved to default fallback host: '{fallback}'");
        return fallback;
    }
}
