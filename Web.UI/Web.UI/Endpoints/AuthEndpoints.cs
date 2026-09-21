using Identifying.Application.Protos;
using Domain.DTOs;

namespace Web.UI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // 🔹 FIDO2 / WebAuthn Passkey Registration Endpoint
        app.MapPost("/api/passkey/register", async (
            PasskeyRegisterDto request,
            AuthService.AuthServiceClient authClient,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            try
            {
                var headers = new Grpc.Core.Metadata();
                if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrWhiteSpace(authHeader))
                {
                    headers.Add("Authorization", authHeader.ToString());
                }

                await authClient.RegisterPasskeyAsync(new PasskeyRegisterCredentials
                {
                    UserId = request.UserId ?? string.Empty,
                    Id = request.Id,
                    RawId = request.RawId,
                    ClientDataJson = request.ClientDataJSON,
                    AttestationObject = request.AttestationObject
                }, headers, cancellationToken: ct);

                return Results.Ok(new { success = true, message = "Passkey успешно зарегистрирован." });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        // 🔹 FIDO2 / WebAuthn Passkey Login Verification Endpoint
        app.MapPost("/api/passkey/verify-login", async (
            PasskeyLoginDto request,
            AuthService.AuthServiceClient authClient,
            CancellationToken ct) =>
        {
            try
            {
                var reply = await authClient.AuthenticateWithPasskeyAsync(new PasskeyAuthCredentials
                {
                    Id = request.Id,
                    RawId = request.RawId,
                    ClientDataJson = request.ClientDataJSON,
                    AuthenticatorData = request.AuthenticatorData,
                    Signature = request.Signature,
                    UserHandle = request.UserHandle ?? string.Empty
                }, cancellationToken: ct);

                return Results.Ok(new { success = true, token = reply.Token });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        // 🔹 Telegram Login Widget Callback API Endpoint
        app.MapPost("/api/auth/telegram-callback", async (
            TelegramAuthDto request,
            AuthService.AuthServiceClient authClient,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            try
            {
                var isLink = httpContext.Request.Query.TryGetValue("state", out var state) && state == "link";
                var headers = new Grpc.Core.Metadata();
                if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrWhiteSpace(authHeader))
                {
                    headers.Add("Authorization", authHeader.ToString());
                }

                if (isLink)
                {
                    await authClient.LinkTelegramAsync(new TelegramAuthCredentials
                    {
                        Id = request.Id,
                        FirstName = request.FirstName,
                        LastName = request.LastName ?? string.Empty,
                        Username = request.Username ?? string.Empty,
                        PhotoUrl = request.PhotoUrl ?? string.Empty,
                        AuthDate = request.AuthDate,
                        Hash = request.Hash
                    }, headers, cancellationToken: ct);

                    return Results.Ok(new { success = true, linked = true });
                }
                else
                {
                    var reply = await authClient.AuthenticateWithTelegramAsync(new TelegramAuthCredentials
                    {
                        Id = request.Id,
                        FirstName = request.FirstName,
                        LastName = request.LastName ?? string.Empty,
                        Username = request.Username ?? string.Empty,
                        PhotoUrl = request.PhotoUrl ?? string.Empty,
                        AuthDate = request.AuthDate,
                        Hash = request.Hash
                    }, cancellationToken: ct);

                    return Results.Ok(new { success = true, token = reply.Token });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });
    }
}
