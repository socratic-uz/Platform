using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Gateways.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Authentication;

namespace Native.UI.Services;

public sealed class MauiExternalAuthBrowser : IExternalAuthBrowser
{
    private readonly ILogger<MauiExternalAuthBrowser> _logger;

    public MauiExternalAuthBrowser(
        IConfiguration configuration,
        ILogger<MauiExternalAuthBrowser> logger)
    {
        _logger = logger;
        var configured = configuration["Auth:PublicOrigin"] ?? configuration["ApiBaseUrl"];
        PublicOrigin = !string.IsNullOrWhiteSpace(configured) && configured.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? configured.TrimEnd('/')
            : "https://socratic.uz";
    }

    public bool IsSupported => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst();

    public string PublicOrigin { get; }

    public async Task<ExternalAuthResult?> AuthenticateAsync(Uri authorizeUrl, Uri callbackUrl)
    {
        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = authorizeUrl,
                    CallbackUrl = callbackUrl,
                    PrefersEphemeralWebBrowserSession = false
                });

            return new ExternalAuthResult(new Dictionary<string, string>(result.Properties, StringComparer.OrdinalIgnoreCase));
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("[MauiExternalAuth] User canceled system browser authentication.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MauiExternalAuth] System browser authentication failed.");
            return null;
        }
    }
}
