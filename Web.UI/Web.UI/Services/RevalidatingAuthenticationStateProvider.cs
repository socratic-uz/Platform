using System.Diagnostics;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;


namespace Web.UI.Services
{
    internal sealed class RevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory)
        : RevalidatingServerAuthenticationStateProvider(loggerFactory)
    {
        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken) => await ValidateSecurityStampAsync(authenticationState.User);

        private async Task<bool> ValidateSecurityStampAsync(ClaimsPrincipal principal) => await Task.FromResult(principal.Identity?.IsAuthenticated is true);
    }
}
