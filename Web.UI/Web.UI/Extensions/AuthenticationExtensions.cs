using Microsoft.AspNetCore.Identity;

using Web.UI.Services;


public static partial class AuthenticationExtensions
{
    public static void AddAuthentication(this IServiceCollection services)
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                o.Cookie.Name = "AccessToken";
                o.TicketDataFormat = new SecureDataFormat(configuration, IdentityConstants.ApplicationScheme);
                o.LoginPath = new PathString("/signin");
            });

    }
}
