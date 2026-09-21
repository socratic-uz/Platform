using System.Security.Claims;
using System.Text;
using System.Text.Json;

using SharedKernel.ValueObjects;

namespace Shared.Services
{
    public interface ISettingsManager
    {
        Task<Theme> GetThemeAsync();
        Task SetThemeAsync(Theme theme);

        Task<int> GetAccentAsync();
        Task SetAccentAsync(int accent);

        Task<Language> GetLanguageAsync();
        Task SetLanguageAsync(Language language);

        Task<string?> GetAccessTokenAsync();
        Task SetAccessTokenAsync(string accessToken);


        async Task<Permission> GetPermissionAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return default;
            var parts = token.Split('.');
            if (parts.Length < 2) return default;
            try
            {
                var encodedPayload = parts[1];
                encodedPayload = encodedPayload.PadRight(encodedPayload.Length + (4 - encodedPayload.Length % 4) % 4, '=');
                var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
                var payload = JsonSerializer.Deserialize(decodedPayload, SharedKernel.Serialization.BackendJsonContext.Default.Payload);
                long.TryParse(payload?.Permission, out var permissionValue);
                return (Permission)permissionValue;
            }
            catch
            {
                return default;
            }
        }

        async Task<string> GetOrganizationIdAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var parts = token.Split('.');
            if (parts.Length < 2) return string.Empty;
            try
            {
                var encodedPayload = parts[1];
                encodedPayload = encodedPayload.PadRight(encodedPayload.Length + (4 - encodedPayload.Length % 4) % 4, '=');
                var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
                var payload = JsonSerializer.Deserialize(decodedPayload, SharedKernel.Serialization.BackendJsonContext.Default.Payload);
                return payload?.OrganizationId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        async Task<string> GetUserIdAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var parts = token.Split('.');
            if (parts.Length < 2) return string.Empty;
            try
            {
                var encodedPayload = parts[1];
                encodedPayload = encodedPayload.PadRight(encodedPayload.Length + (4 - encodedPayload.Length % 4) % 4, '=');
                var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
                var payload = JsonSerializer.Deserialize(decodedPayload, SharedKernel.Serialization.BackendJsonContext.Default.Payload);
                return payload?.UserId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        async Task<string> GetRoleIdAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var parts = token.Split('.');
            if (parts.Length < 2) return string.Empty;
            try
            {
                var encodedPayload = parts[1];
                encodedPayload = encodedPayload.PadRight(encodedPayload.Length + (4 - encodedPayload.Length % 4) % 4, '=');
                string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
                var payload = JsonSerializer.Deserialize(decodedPayload, SharedKernel.Serialization.BackendJsonContext.Default.Payload);
                return payload?.RoleId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}