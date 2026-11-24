using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Web.UI.Services;

internal sealed class RevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState state, CancellationToken ct) =>
        Task.FromResult(IsValid(state.User));

    private static bool IsValid(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated is true
        && (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out var exp)
            || exp > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        && (user.FindFirst("UserId")?.Value is not { } uid
            || (Guid.TryParse(uid, out var id) && id != Guid.Empty));
}
