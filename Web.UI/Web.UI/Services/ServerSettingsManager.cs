using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using SharedKernel.ValueObjects;
using Shared.Extensions;
using Shared.Services;

namespace Web.UI.Services
{
    public class ServerSettingsManager : ISettingsManager
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IServiceProvider _serviceProvider;

        public ServerSettingsManager(IHttpContextAccessor accessor, IServiceProvider serviceProvider)
        {
            _accessor = accessor;
            _serviceProvider = serviceProvider;
        }

        public Task<Theme> GetThemeAsync()
        {
            var theme = _accessor.HttpContext?.Request?.Cookies[nameof(Theme)];
            Enum.TryParse<Theme>(theme, true, out var parsedTheme);
            return Task.FromResult(parsedTheme);
        }

        public Task SetThemeAsync(Theme theme)
        {
            if (_accessor.HttpContext?.Response != null && !_accessor.HttpContext.Response.HasStarted)
            {
                _accessor.HttpContext.Response.Cookies.Append(nameof(Theme), theme.ToString(), new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
            return Task.CompletedTask;
        }

        public Task<Language> GetLanguageAsync()
        {
            var language = _accessor.HttpContext?.Request?.Cookies[nameof(Language)];
            Enum.TryParse<Language>(language, true, out var parsedLanguage);
            return Task.FromResult(parsedLanguage);
        }

        public Task SetLanguageAsync(Language language)
        {
            if (_accessor.HttpContext?.Response != null && !_accessor.HttpContext.Response.HasStarted)
            {
                _accessor.HttpContext.Response.Cookies.Append(nameof(Language), language.ToString(), new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
            return Task.CompletedTask;
        }

        public Task<int> GetAccentAsync()
        {
            var accentStr = _accessor.HttpContext?.Request?.Cookies["Accent"];
            return Task.FromResult(SettingsManager.ParseAccentCookie(accentStr));
        }

        public Task SetAccentAsync(int accent)
        {
            if (_accessor.HttpContext?.Response != null && !_accessor.HttpContext.Response.HasStarted)
            {
                accent = SettingsManager.NormalizeAccent(accent);
                _accessor.HttpContext.Response.Cookies.Append("Accent", accent.ToString(), new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
            return Task.CompletedTask;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var accessToken = _accessor.HttpContext?.Request?.Cookies[AuthenticationExtensions.AccessTokenCookie];
            if (!string.IsNullOrWhiteSpace(accessToken))
                return Uri.UnescapeDataString(accessToken);

            try
            {
                if (_serviceProvider.GetService<IJSRuntime>() is { } js)
                    return await js.InvokeAsync<string?>("oauthPopupHelper.getAccessToken");
            }
            catch { }

            return null;
        }

        public async Task SetAccessTokenAsync(string accessToken)
        {
            if (_accessor.HttpContext?.Response is { HasStarted: false } response)
            {
                try
                {
                    response.Cookies.Append(AuthenticationExtensions.AccessTokenCookie, accessToken ?? string.Empty, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = _accessor.HttpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = string.IsNullOrEmpty(accessToken) ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddDays(30)
                    });
                }
                catch { }
            }

            try
            {
                if (_serviceProvider.GetService<IJSRuntime>() is { } js)
                {
                    if (string.IsNullOrEmpty(accessToken))
                        await js.InvokeVoidAsync("oauthPopupHelper.clearAccessToken");
                    else
                        await js.InvokeVoidAsync("oauthPopupHelper.setAccessToken", accessToken);
                }
            }
            catch { }
        }
    }
}
