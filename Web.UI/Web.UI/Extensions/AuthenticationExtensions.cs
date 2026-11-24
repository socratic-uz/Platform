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
                o.TicketDataFormat = new SecureDataFormat(builder.Configuration, IdentityConstants.ApplicationScheme);
            });
    }
}
