namespace Infrastructure.Gateways.Identity;

using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Interfaces.Identity;
using Grpc.Core;
using Identifying.Application.Protos;
using Shared.Services;

/// <summary>
/// Infrastructure-реализация шлюза аккаунта через AuthService.AuthServiceClient.
/// </summary>
public sealed class UserAccountGateway : IUserAccountGateway
{
    private readonly AuthService.AuthServiceClient? _authClient;
    private readonly ISettingsManager? _settings;

    public UserAccountGateway(AuthService.AuthServiceClient? authClient = null, ISettingsManager? settings = null)
    {
        _authClient = authClient;
        _settings = settings;
    }

    public async ValueTask<string?> SwitchRoleAsync(string roleId, CancellationToken ct = default)
    {
        if (_authClient == null) return null;
        try
        {
            var access = await _authClient.SwitchAsync(new SwitchRequest { RoleId = roleId }, cancellationToken: ct);
            return access?.Token;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask<bool> RegisterPasskeyAsync(
        string userId,
        string id,
        string rawId,
        string clientDataJson,
        string attestationObject,
        CancellationToken ct = default)
    {
        if (_authClient == null) return false;
        try
        {
            var headers = new Metadata();
            if (_settings != null)
            {
                var token = await _settings.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    headers.Add("Authorization", $"Bearer {token}");
                }
            }

            await _authClient.RegisterPasskeyAsync(new PasskeyRegisterCredentials
            {
                UserId = userId,
                Id = id,
                RawId = rawId,
                ClientDataJson = clientDataJson,
                AttestationObject = attestationObject
            }, headers, cancellationToken: ct);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
