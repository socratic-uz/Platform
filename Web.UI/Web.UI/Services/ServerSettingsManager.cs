using Microsoft.AspNetCore.Http;
using SharedKernel.ValueObjects;
using Shared.Services;

namespace Web.UI.Services
{
    public class ServerSettingsManager : ISettingsManager
    {
        private readonly IHttpContextAccessor _accessor;

        public ServerSettingsManager(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
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

        public Task<string?> GetAccessTokenAsync()
        {
            var accessToken = _accessor.HttpContext?.Request?.Cookies["AccessToken"];
            return Task.FromResult(accessToken);
        }

        public Task SetAccessTokenAsync(string accessToken)
        {
            if (_accessor.HttpContext?.Response != null && !_accessor.HttpContext.Response.HasStarted)
            {
                _accessor.HttpContext.Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
            return Task.CompletedTask;
        }
    }
}
