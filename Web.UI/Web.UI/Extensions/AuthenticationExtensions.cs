using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Web.UI.Services;

public static partial class AuthenticationExtensions
{
    public const string AccessTokenCookie = "AccessToken";

    public static void AddAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDataProtection()
            .SetApplicationName(builder.Environment.ApplicationName)
            .AddKeyManagementOptions(o =>
            {
                o.XmlRepository = new DeterministicJwtKeyXmlRepository(builder.Configuration);
                o.AutoGenerateKeys = false;
            });

        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                o.Cookie.Name = AccessTokenCookie;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                o.LoginPath = new PathString("/signin");
                o.TicketDataFormat = new SecureDataFormat(builder.Configuration, IdentityConstants.ApplicationScheme);
                o.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        var path = $"{ctx.Request.PathBase}{ctx.Request.Path}";
                        var query = ctx.Request.QueryString.HasValue ? Uri.EscapeDataString(ctx.Request.QueryString.Value!) : string.Empty;
                        var returnUrl = string.IsNullOrEmpty(path) ? "/" : $"{path}{query}";
                        ctx.Response.Redirect($"{ctx.Request.PathBase}/signin?ReturnUrl={returnUrl}");
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
