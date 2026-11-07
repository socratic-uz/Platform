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
            var accessToken = _accessor.HttpContext?.Request?.Cookies["AccessToken"];
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            try
            {
                var js = _serviceProvider.GetService<IJSRuntime>();
                if (js != null)
                {
                    return await js.GetAsync("AccessToken");
                }
            }
            catch
            {
                // Fallback for cases where JS interop is not connected or prerendering
            }

            return null;
        }

        public async Task SetAccessTokenAsync(string accessToken)
        {
            if (_accessor.HttpContext?.Response != null && !_accessor.HttpContext.Response.HasStarted)
            {
                try
                {
                    _accessor.HttpContext.Response.Cookies.Append("AccessToken", accessToken ?? string.Empty, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = string.IsNullOrEmpty(accessToken) ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddDays(30)
                    });
                }
                catch
                {
                    // Response headers may have started in interactive mode
                }
            }

            // Always also write via JS Interop when in interactive SignalR / client mode or if Response already started
            try
            {
                var js = _serviceProvider.GetService<IJSRuntime>();
                if (js != null)
                {
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        await js.InvokeVoidAsync("eval", "document.cookie = 'AccessToken=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT; SameSite=Lax; Secure'; try { localStorage.removeItem('AccessToken'); sessionStorage.removeItem('AccessToken'); } catch(e){}");
                    }
                    else
                    {
                        var escaped = Uri.EscapeDataString(accessToken);
                        await js.InvokeVoidAsync("eval", "document.cookie = 'AccessToken=" + escaped + "; path=/; max-age=2592000; SameSite=Lax; Secure'; try { localStorage.setItem('AccessToken', '" + escaped + "'); } catch(e){}");
                    }
                }
            }
            catch
            {
                // Prerendering fallback where JS is not yet interactive
            }
        }
    }
}
