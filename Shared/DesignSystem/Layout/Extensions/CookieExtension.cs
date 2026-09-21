using System.ComponentModel;
using System.Linq;
using System.Text.Json;

using Microsoft.JSInterop;

namespace Shared.Extensions
{
    public static class CookieExtension
    {
        public static AltairCABlazorCookieConfigOptions _settings = new AltairCABlazorCookieConfigOptions();

        /// <summary>
        /// Set a object in the cookie
        /// </summary>
        /// <param name="key">The key for the cookie (name)</param>
        /// <param name="value">Cookie value</param>
        /// <param name="span">TimeSpan that will be set to the 'expires' attribute value</param>
        /// <param name="path">Path in the request url which must exist for the cookie to be sent in requests </param>
        /// <param name="domain">The host to which the cookie will be sent</param>
        /// <param name="secure">Specifies that the cookie will be sent only over secure protocols</param>
        /// <param name="isSession">Flags the cookie as a session cookie (temporal) by setting the 'expires' attribute value to ''</param>
        /// <param name="partitioned">Requires that the browser has activated partitioned cookies</param>
        /// <param name="maxAgeInSeconds">Maximum age in seconds</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Cookie objects deserialization uses preserved application DTO types")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "Cookie objects deserialization uses preserved application DTO types")]
        public static async Task SetAsync(this IJSRuntime js, string key, object value, TimeSpan? span = null, string? path = null, string? domain = null, bool? secure = null, SameSite? sameSite = null, bool? partitioned = null, bool? isSession = null, int? maxAgeInSeconds = 2592000)
        {
            await js.SetAsync(key, JsonSerializer.Serialize(value), span, path, domain, secure, sameSite, partitioned,
                isSession, maxAgeInSeconds);
        }
        /// <summary>
        /// Set a string in the cookie
        /// </summary>
        /// <param name="key">The key for the cookie (name)</param>
        /// <param name="value">Cookie value</param>
        /// <param name="span">TimeSpan that will be set to the 'expires' attribute value</param>
        /// <param name="path">Path in the request url which must exist for the cookie to be sent in requests </param>
        /// <param name="domain">The host to which the cookie will be sent</param>
        /// <param name="secure">Specifies that the cookie will be sent only over secure protocols</param>
        /// <param name="isSession">Flags the cookie as a session cookie (temporal) by setting the 'expires' attribute value to ''</param>
        /// <param name="partitioned">Requires that the browser has activated partitioned cookies</param>
        /// <param name="maxAgeInSeconds">Maximum age in seconds</param>
        /// <returns></returns>
        public static async Task SetAsync(this IJSRuntime js, string key, string value, TimeSpan? span = null, string? path = null, string? domain = null, bool? secure = null, SameSite? sameSite = null, bool? partitioned = null, bool? isSession = null, int? maxAgeInSeconds = 2592000)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = _settings.Path;
            if (string.IsNullOrWhiteSpace(path))
                path = "/";
            if (!span.HasValue)
                span = _settings.DefaultExpire;
            if (string.IsNullOrWhiteSpace(domain))
                domain = _settings.Domain;
            if (!secure.HasValue)
                secure = _settings.IsSecure;
            if (!sameSite.HasValue)
                sameSite = SameSite.Lax;

            var curExp = span.HasValue && span.Value.Ticks > 0 && isSession != true && !maxAgeInSeconds.HasValue ? DateToUTC(span.Value) : "";

            List<string> keyvals = new List<string>();
            keyvals.Add($"{key}={value}");
            if (!string.IsNullOrWhiteSpace(curExp))
            {
                keyvals.Add($"expires={curExp}");
            }
            keyvals.Add($"path={path}");
            if (!string.IsNullOrWhiteSpace(domain))
                keyvals.Add($"domain={domain}");
            if (secure.HasValue && secure.Value)
                keyvals.Add("secure");
            if (maxAgeInSeconds.HasValue && isSession != true)
            {
                keyvals.Add($"max-age={maxAgeInSeconds.Value}");
            }
            if (sameSite.HasValue)
            {
                DescriptionAttribute desc = (DescriptionAttribute)typeof(SameSite).GetMember(sameSite.Value.ToString()).First().GetCustomAttributes(typeof(DescriptionAttribute), false).First();
                keyvals.Add($"samesite={desc.Description}");
            }
            string cookieToSet = string.Join(";", keyvals);
            if (partitioned == true)
            {
                cookieToSet += ";partitioned";
            }

            await js.SetAsync(cookieToSet);
        }
        public static async Task RemoveAsync(this IJSRuntime js, string key, string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = _settings.Path;
            if (string.IsNullOrWhiteSpace(path))
                path = "/";
            List<string> keyvals = new List<string>();
            keyvals.Add($"{key}=");
            keyvals.Add($"path={path}");
            keyvals.Add($"expires=Thu, 01 Jan 1970 00:00:01 GMT");
            keyvals.Add("samesite=lax");
            await js.SetAsync(string.Join(";", keyvals));
        }
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Cookie objects deserialization uses preserved application DTO types")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "Cookie objects deserialization uses preserved application DTO types")]
        public static async Task<T?> GetAsync<T>(this IJSRuntime js, string key) where T : class
        {

            var res = await GetAsync(js, key);
            if (res == null)
                return default(T);
            return JsonSerializer.Deserialize<T>(res);
        }
        public static async Task<string?> GetAsync(this IJSRuntime js, string key)
        {
            var cValue = await js.GetAsync();

            if (string.IsNullOrEmpty(cValue)) return null;

            var vals = cValue.Split(';');
            foreach (var val in vals)
                if (!string.IsNullOrEmpty(val) && val.IndexOf('=') > 0)
                    if (val.Substring(0, val.IndexOf('=')).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return val.Substring(val.IndexOf('=') + 1);
            return null;
        }
        public static async ValueTask SetAsync(this IJSRuntime js, string value)
        {
            await js.InvokeVoidAsync("eval", $"document.cookie = \'{value}\'");
        }
        public static async Task<string> GetAsync(this IJSRuntime js)
        {
            return await js.InvokeAsync<string>("eval", $"document.cookie");
        }
        public static string DateToUTC(TimeSpan span) => DateTime.Now.Add(span).ToUniversalTime().ToString("R");
    }


    public class AltairCABlazorCookieConfigOptions
    {
        public TimeSpan DefaultExpire { get; set; } = TimeSpan.Zero;
        public string Path { get; set; } = "/";
        public string Domain { get; set; } = string.Empty;
        public bool IsSecure { get; set; } = false;
    }



    /// <summary>
    /// We also have this enum used in cookie handler class above
    /// </summary>
    public enum SameSite
    {
        [Description("lax")] Lax = 0,
        [Description("strict")] Strict = 1,
        [Description("none")] None = 2
    }

}
