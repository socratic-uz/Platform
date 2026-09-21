using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using SharedKernel.ValueObjects;

namespace Shared.Extensions
{
    public static class AuthenticationStateExtension
    {
        public static async Task<bool> IsAuthenticatedAsync(this AuthenticationStateProvider authenticationState)
            => (await authenticationState.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated ?? false;

        public static async Task<Guid> GetUserIdAsync(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse((await authenticationState.GetAuthenticationStateAsync())
                .User.FindFirst("UserId")?.Value, out var userId) ? userId : default;

        public static async Task<Guid> GetRoleIdAsync(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse((await authenticationState.GetAuthenticationStateAsync())
                .User.FindFirst("RoleId")?.Value, out var roleId) ? roleId : default;

        public static async Task<Guid> GetOrganizationIdAsync(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse((await authenticationState.GetAuthenticationStateAsync())
                .User.FindFirst("OrganizationId")?.Value, out var organizationId) ? organizationId : default;

        public static async Task<Permission> GetPermissionAsync(this AuthenticationStateProvider authenticationState)
            => Enum.TryParse<Permission>((await authenticationState.GetAuthenticationStateAsync())
                .User.FindFirst("Permission")?.Value, out var permission) ? permission : default;

        public static Guid GetUserId(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse(authenticationState.GetAuthenticationStateAsync().Result
                .User.FindFirst("UserId")?.Value, out var userId) ? userId : default;

        public static Guid GetRoleId(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse(authenticationState.GetAuthenticationStateAsync().Result
                .User.FindFirst("RoleId")?.Value, out var roleId) ? roleId : default;
        public static Guid GetOrganizationId(this AuthenticationStateProvider authenticationState)
            => Guid.TryParse(authenticationState.GetAuthenticationStateAsync().Result
                .User.FindFirst("OrganizationId")?.Value, out var organizationId) ? organizationId : default;

        public static Permission GetPermission(this AuthenticationStateProvider authenticationState)
            => Enum.TryParse<Permission>(authenticationState.GetAuthenticationStateAsync().Result
                .User.FindFirst("Permission")?.Value, out var permission) ? permission : default;
    }
}
