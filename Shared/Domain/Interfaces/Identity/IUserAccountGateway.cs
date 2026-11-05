namespace Domain.Interfaces.Identity;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Domain-интерфейс шлюза аккаунта и профиля пользователя (смена ролей, регистрация биометрии/Passkey).
/// 100% Native AOT совместимый, без зависимостей от gRPC/Protobuf.
/// </summary>
public interface IUserAccountGateway
{
    ValueTask<string?> SwitchRoleAsync(string roleId, CancellationToken ct = default);

    ValueTask<bool> RegisterPasskeyAsync(
        string userId,
        string id,
        string rawId,
        string clientDataJson,
        string attestationObject,
        CancellationToken ct = default);
}
