using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using static SharedKernel.Options.JwtOptions;
using static SharedKernel.ValueObjects.EnvironmentVariables;

namespace Web.UI.Services;

public sealed class SecureDataFormat : ISecureDataFormat<AuthenticationTicket>
{
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _validationParameters;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly string _scheme;
    private readonly TimeSpan _lifetime;

    public SecureDataFormat(IConfiguration configuration, string authenticationScheme)
    {
        _configuration = configuration;
        _scheme = authenticationScheme;
        var key = configuration[JWT_KEY] ?? throw new InvalidOperationException($"{JWT_KEY} is required.");
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        _lifetime = int.TryParse(configuration[JWT_LIFETIME], out var minutes) && minutes > 0
            ? TimeSpan.FromMinutes(minutes)
            : TimeSpan.FromDays(30);

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    public string Protect(AuthenticationTicket data) =>
        _tokenHandler.WriteToken(_tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(data.Principal.Claims),
            Issuer = _configuration[IDENTIFYING_SERVICE_URL],
            Audience = _configuration[WEB_UI_URL],
            Expires = DateTime.UtcNow.Add(_lifetime),
            SigningCredentials = _signingCredentials
        }));

    public string Protect(AuthenticationTicket data, string? purpose) => Protect(data);

    public AuthenticationTicket? Unprotect(string? protectedText)
    {
        if (string.IsNullOrWhiteSpace(protectedText)) return null;
        var raw = protectedText.Contains('%') ? Uri.UnescapeDataString(protectedText) : protectedText;
        try
        {
            return new AuthenticationTicket(_tokenHandler.ValidateToken(raw, _validationParameters, out _), _scheme);
        }
        catch
        {
            return null;
        }
    }

    public AuthenticationTicket? Unprotect(string? protectedText, string? purpose) => Unprotect(protectedText);
}
