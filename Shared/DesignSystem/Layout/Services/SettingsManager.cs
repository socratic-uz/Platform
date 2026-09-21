using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SharedKernel.ValueObjects;
using Shared.Extensions;

namespace Shared.Services
{
    public class SettingsManager : ISettingsManager
    {
        private readonly ISettingsManager _settingsManager;

        public SettingsManager(IJSRuntime js)
        {
            _settingsManager = new JSRuntimeSettingsManager(js);
        }

        public Task<Theme> GetThemeAsync() => _settingsManager.GetThemeAsync();
        public Task SetThemeAsync(Theme theme) => _settingsManager.SetThemeAsync(theme);
        public Task<Language> GetLanguageAsync() => _settingsManager.GetLanguageAsync();
        public Task SetLanguageAsync(Language language) => _settingsManager.SetLanguageAsync(language);
        public Task<int> GetAccentAsync() => _settingsManager.GetAccentAsync();
        public Task SetAccentAsync(int accent) => _settingsManager.SetAccentAsync(accent);
        public Task<string?> GetAccessTokenAsync() => _settingsManager.GetAccessTokenAsync();
        public Task SetAccessTokenAsync(string accessToken) => _settingsManager.SetAccessTokenAsync(accessToken);

        public static int NormalizeAccent(int val)
        {
            if (val <= 3)
            {
                return val switch
                {
                    0 => 65459,   // #00ffb3
                    1 => 3899638, // #3b82f6
                    2 => 15680580,// #ef4444
                    3 => 15381256,// #eab308
                    _ => 65459
                };
            }
            return val & 0xFFFFFF;
        }

        public static int ParseAccentCookie(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 65459;
            raw = raw.Trim();

            // 1. Explicit HEX with '#' prefix (e.g. "#00ffb3")
            if (raw.StartsWith("#"))
            {
                if (int.TryParse(raw.TrimStart('#'), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hexVal))
                    return NormalizeAccent(hexVal);
            }

            // 2. Decimal integer string (e.g. "65459")
            if (int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var decVal))
            {
                return NormalizeAccent(decVal);
            }

            // 3. Fallback: HEX string without '#' (e.g. "00ffb3")
            if (int.TryParse(raw, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var rawHexVal))
            {
                return NormalizeAccent(rawHexVal);
            }

            return 65459;
        }

        public class JSRuntimeSettingsManager : ISettingsManager
        {
            private readonly IJSRuntime _js;

            public JSRuntimeSettingsManager(IJSRuntime js)
            {
                _js = js ?? throw new ArgumentNullException(nameof(js));
            }

            public async Task<Theme> GetThemeAsync()
            {
                try
                {
                    var theme = await _js.GetAsync(nameof(Theme));
                    Enum.TryParse<Theme>(theme, true, out var parsedTheme);
                    return parsedTheme;
                }
                catch { return Theme.Dark; }
            }

            public async Task SetThemeAsync(Theme theme)
            {
                try { await _js.SetAsync(nameof(Theme), theme.ToString()); } catch { }
            }

            public async Task<Language> GetLanguageAsync()
            {
                try
                {
                    var language = await _js.GetAsync(nameof(Language));
                    Enum.TryParse<Language>(language, true, out var parsedLanguage);
                    return parsedLanguage;
                }
                catch { return Language.Ru; }
            }

            public async Task SetLanguageAsync(Language language)
            {
                try { await _js.SetAsync(nameof(Language), language.ToString()); } catch { }
            }

            public async Task<int> GetAccentAsync()
            {
                try
                {
                    var accent = await _js.GetAsync("Accent");
                    return ParseAccentCookie(accent);
                }
                catch { return 65459; }
            }

            public async Task SetAccentAsync(int accent)
            {
                try
                {
                    accent = NormalizeAccent(accent);
                    await _js.SetAsync("Accent", accent.ToString());
                } catch { }
            }

            public async Task<string?> GetAccessTokenAsync()
            {
                try { return await _js.GetAsync("AccessToken"); } catch { return null; }
            }

            public async Task SetAccessTokenAsync(string accessToken)
            {
                try { await _js.SetAsync("AccessToken", accessToken); } catch { }
            }
        }
    }
}