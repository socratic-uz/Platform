using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static SharedKernel.Options.JwtOptions;
using static SharedKernel.ValueObjects.EnvironmentVariables;

namespace Web.UI.Services
{
    public class SecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _validationParameters;
        private readonly SecurityTokenDescriptor _tokenDescriptor;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly string _authenticationScheme;

        public SecureDataFormat(IConfiguration configuration, string authenticationScheme)
        {
            _configuration = configuration;
            string key = _configuration[JWT_KEY] ?? throw new ArgumentNullException("JWT_KEY is missing in appsettings.json");
            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
            _tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = _configuration[IDENTIFYING_SERVICE_URL],
                Audience = _configuration[WEB_UI_URL],
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(_validationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
            };
            _authenticationScheme = authenticationScheme;
        }

        public string Protect(AuthenticationTicket data)
        {
            _tokenDescriptor.Subject = new ClaimsIdentity(data.Principal.Claims);
            var token = _tokenHandler.CreateToken(_tokenDescriptor);

            return _tokenHandler.WriteToken(token);
        }

        public string Protect(AuthenticationTicket data, string? purpose) => Protect(data);

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
            var tokenString = protectedText;
            try
            {
                var principal = _tokenHandler.ValidateToken(tokenString, _validationParameters, out var validatedToken);
                return new AuthenticationTicket(principal, _authenticationScheme);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose) => Unprotect(protectedText);
    }
}