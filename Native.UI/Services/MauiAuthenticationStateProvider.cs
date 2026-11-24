using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Services;

namespace Native.UI.Services
{
    public class MauiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ISettingsManager _settingsManager;
        private static readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public MauiAuthenticationStateProvider(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _settingsManager.GetAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new AuthenticationState(_anonymous);
                }

                var principal = CreatePrincipalFromToken(token);
                return new AuthenticationState(principal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var principal = CreatePrincipalFromToken(token);
            var authState = Task.FromResult(new AuthenticationState(principal));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private static ClaimsPrincipal CreatePrincipalFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return _anonymous;
            }
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var parts = jwt.Split('.');
            if (parts.Length < 2) return claims;

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            using var doc = JsonDocument.Parse(jsonBytes);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return claims;

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(prop.Name, item.ToString()));
                    }
                }
                else if (prop.Value.ValueKind != JsonValueKind.Null && prop.Value.ValueKind != JsonValueKind.Undefined)
                {
                    claims.Add(new Claim(prop.Name, prop.Value.ToString()));
                }
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
        }
    }
}
