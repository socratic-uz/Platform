using System;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Infrastructure.Gateways.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Native.UI.Services;

public sealed class MauiPasskeyService : IPasskeyService
{
    private const string DefaultRpId = "socratic.uz";
    private const string DefaultRpName = "Socratic ID";

    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<MauiPasskeyService> _logger;
    private readonly string _rpId;

    public MauiPasskeyService(
        IJSRuntime jsRuntime,
        IConfiguration configuration,
        ILogger<MauiPasskeyService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _rpId = configuration["Auth:PasskeyRpId"] ?? DefaultRpId;
    }

    public async ValueTask<bool> IsSupportedAsync()
    {
#if ANDROID
        return Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P;
#elif IOS || MACCATALYST
        return OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16);
#else
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("passkeyHelper.isSupported");
        }
        catch
        {
            return false;
        }
#endif
    }

    public async ValueTask<string?> RegisterAsync(string userId, string userName, string challengeBase64)
    {
#if ANDROID
        return await RegisterAndroidAsync(userId, userName, challengeBase64);
#elif IOS || MACCATALYST
        return await RegisterAppleAsync(userId, userName, challengeBase64);
#else
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("passkeyHelper.register", userId, userName, challengeBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MauiPasskey] JS fallback RegisterAsync failed.");
            return null;
        }
#endif
    }

    public async ValueTask<string?> AuthenticateAsync(string challengeBase64)
    {
#if ANDROID
        return await AuthenticateAndroidAsync(challengeBase64);
#elif IOS || MACCATALYST
        return await AuthenticateAppleAsync(challengeBase64);
#else
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("passkeyHelper.authenticate", challengeBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MauiPasskey] JS fallback AuthenticateAsync failed.");
            return null;
        }
#endif
    }

    private static string ToBase64Url(byte[] bytes)
        => Base64Url.EncodeToString(bytes);

    private static string NormalizeChallengeToBase64Url(string challengeBase64)
    {
        try
        {
            var bytes = Convert.FromBase64String(challengeBase64);
            return ToBase64Url(bytes);
        }
        catch
        {
            return challengeBase64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }

    private static byte[] DecodeChallengeBytes(string challengeBase64)
    {
        try
        {
            return Convert.FromBase64String(challengeBase64);
        }
        catch
        {
            return Base64Url.DecodeFromChars(challengeBase64.AsSpan());
        }
    }

#if ANDROID
    private async Task<string?> RegisterAndroidAsync(string userId, string userName, string challengeBase64)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is null)
        {
            _logger.LogWarning("[MauiPasskey.Android] CurrentActivity is null.");
            return null;
        }

        try
        {
            var requestJson = BuildCreateOptionsJson(userId, userName, challengeBase64);
            var manager = AndroidX.Credentials.CredentialManager.Create(activity);
            var request = new AndroidX.Credentials.CreatePublicKeyCredentialRequest(requestJson);
            var tcs = new TaskCompletionSource<Java.Lang.Object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callback = new CredentialManagerCallback(tcs);

            manager.CreateCredentialAsync(activity, request, null, activity.MainExecutor!, callback);
            var result = await tcs.Task.ConfigureAwait(false);

            if (result is AndroidX.Credentials.CreatePublicKeyCredentialResponse createResp)
            {
                return FlattenRegistrationResponse(createResp.RegistrationResponseJson);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MauiPasskey.Android] Register failed or canceled.");
            return null;
        }
    }

    private async Task<string?> AuthenticateAndroidAsync(string challengeBase64)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is null)
        {
            _logger.LogWarning("[MauiPasskey.Android] CurrentActivity is null.");
            return null;
        }

        try
        {
            var requestJson = BuildGetOptionsJson(challengeBase64);
            var manager = AndroidX.Credentials.CredentialManager.Create(activity);
            var option = new AndroidX.Credentials.GetPublicKeyCredentialOption(requestJson);
            var request = new AndroidX.Credentials.GetCredentialRequest.Builder()
                .AddCredentialOption(option)
                .Build();

            var tcs = new TaskCompletionSource<Java.Lang.Object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callback = new CredentialManagerCallback(tcs);

            manager.GetCredentialAsync(activity, request, null, activity.MainExecutor!, callback);
            var result = await tcs.Task.ConfigureAwait(false);

            if (result is AndroidX.Credentials.GetCredentialResponse getResp &&
                getResp.Credential is AndroidX.Credentials.PublicKeyCredential pkCred)
            {
                return FlattenAuthenticationResponse(pkCred.AuthenticationResponseJson);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MauiPasskey.Android] Authenticate failed or canceled.");
            return null;
        }
    }

    private string BuildCreateOptionsJson(string userId, string userName, string challengeBase64)
    {
        var root = new JsonObject
        {
            ["challenge"] = NormalizeChallengeToBase64Url(challengeBase64),
            ["rp"] = new JsonObject
            {
                ["name"] = DefaultRpName,
                ["id"] = _rpId
            },
            ["user"] = new JsonObject
            {
                ["id"] = ToBase64Url(Encoding.UTF8.GetBytes(userId)),
                ["name"] = userName,
                ["displayName"] = userName
            },
            ["pubKeyCredParams"] = new JsonArray
            {
                new JsonObject { ["type"] = "public-key", ["alg"] = -7 },
                new JsonObject { ["type"] = "public-key", ["alg"] = -257 }
            },
            ["timeout"] = 60000,
            ["attestation"] = "none",
            ["authenticatorSelection"] = new JsonObject
            {
                ["residentKey"] = "required",
                ["userVerification"] = "required"
            }
        };
        return root.ToJsonString();
    }

    private string BuildGetOptionsJson(string challengeBase64)
    {
        var root = new JsonObject
        {
            ["challenge"] = NormalizeChallengeToBase64Url(challengeBase64),
            ["rpId"] = _rpId,
            ["timeout"] = 60000,
            ["userVerification"] = "required"
        };
        return root.ToJsonString();
    }

    private static string? FlattenRegistrationResponse(string registrationResponseJson)
    {
        using var doc = JsonDocument.Parse(registrationResponseJson);
        var root = doc.RootElement;
        var response = root.GetProperty("response");

        var flat = new JsonObject
        {
            ["id"] = root.GetProperty("id").GetString(),
            ["rawId"] = root.TryGetProperty("rawId", out var rawId) ? rawId.GetString() : root.GetProperty("id").GetString(),
            ["clientDataJSON"] = response.GetProperty("clientDataJSON").GetString(),
            ["attestationObject"] = response.GetProperty("attestationObject").GetString()
        };
        return flat.ToJsonString();
    }

    private static string? FlattenAuthenticationResponse(string authenticationResponseJson)
    {
        using var doc = JsonDocument.Parse(authenticationResponseJson);
        var root = doc.RootElement;
        var response = root.GetProperty("response");

        var flat = new JsonObject
        {
            ["id"] = root.GetProperty("id").GetString(),
            ["rawId"] = root.TryGetProperty("rawId", out var rawId) ? rawId.GetString() : root.GetProperty("id").GetString(),
            ["clientDataJSON"] = response.GetProperty("clientDataJSON").GetString(),
            ["authenticatorData"] = response.GetProperty("authenticatorData").GetString(),
            ["signature"] = response.GetProperty("signature").GetString(),
            ["userHandle"] = response.TryGetProperty("userHandle", out var userHandle) && userHandle.ValueKind == JsonValueKind.String
                ? userHandle.GetString()
                : string.Empty
        };
        return flat.ToJsonString();
    }

    private sealed class CredentialManagerCallback(TaskCompletionSource<Java.Lang.Object?> tcs)
        : Java.Lang.Object, AndroidX.Credentials.ICredentialManagerCallback
    {
        public void OnResult(Java.Lang.Object? result)
        {
            tcs.TrySetResult(result);
        }

        public void OnError(Java.Lang.Object error)
        {
            tcs.TrySetException(new InvalidOperationException(error.ToString()));
        }
    }
#endif

#if IOS || MACCATALYST
    private async Task<string?> RegisterAppleAsync(string userId, string userName, string challengeBase64)
    {
        try
        {
            var provider = new AuthenticationServices.ASAuthorizationPlatformPublicKeyCredentialProvider(_rpId);
            var challenge = Foundation.NSData.FromArray(DecodeChallengeBytes(challengeBase64));
            var userIdData = Foundation.NSData.FromArray(Encoding.UTF8.GetBytes(userId));
            var request = provider.CreateCredentialRegistrationRequest(challenge, userName, userIdData);

            var authorization = await RunAppleAuthControllerAsync(request).ConfigureAwait(false);
            if (authorization?.Credential is AuthenticationServices.ASAuthorizationPlatformPublicKeyCredentialRegistration reg)
            {
                var idB64 = ToBase64Url(reg.CredentialID.ToArray());
                var flat = new JsonObject
                {
                    ["id"] = idB64,
                    ["rawId"] = idB64,
                    ["clientDataJSON"] = ToBase64Url(reg.RawClientDataJson.ToArray()),
                    ["attestationObject"] = ToBase64Url(reg.RawAttestationObject?.ToArray() ?? [])
                };
                return flat.ToJsonString();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MauiPasskey.Apple] Register failed or canceled.");
            return null;
        }
    }

    private async Task<string?> AuthenticateAppleAsync(string challengeBase64)
    {
        try
        {
            var provider = new AuthenticationServices.ASAuthorizationPlatformPublicKeyCredentialProvider(_rpId);
            var challenge = Foundation.NSData.FromArray(DecodeChallengeBytes(challengeBase64));
            var request = provider.CreateCredentialAssertionRequest(challenge);

            var authorization = await RunAppleAuthControllerAsync(request).ConfigureAwait(false);
            if (authorization?.Credential is AuthenticationServices.ASAuthorizationPlatformPublicKeyCredentialAssertion assert)
            {
                var idB64 = ToBase64Url(assert.CredentialID.ToArray());
                var flat = new JsonObject
                {
                    ["id"] = idB64,
                    ["rawId"] = idB64,
                    ["clientDataJSON"] = ToBase64Url(assert.RawClientDataJson.ToArray()),
                    ["authenticatorData"] = ToBase64Url(assert.RawAuthenticatorData.ToArray()),
                    ["signature"] = ToBase64Url(assert.Signature.ToArray()),
                    ["userHandle"] = assert.UserID is not null ? ToBase64Url(assert.UserID.ToArray()) : string.Empty
                };
                return flat.ToJsonString();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MauiPasskey.Apple] Authenticate failed or canceled.");
            return null;
        }
    }

    private static Task<AuthenticationServices.ASAuthorization?> RunAppleAuthControllerAsync(
        AuthenticationServices.ASAuthorizationRequest request)
    {
        var tcs = new TaskCompletionSource<AuthenticationServices.ASAuthorization?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            var controller = new AuthenticationServices.ASAuthorizationController([request]);
            var authDelegate = new AppleAuthDelegate(tcs);
            controller.Delegate = authDelegate;
            controller.PresentationContextProvider = authDelegate;
            controller.PerformRequests();
        });
        return tcs.Task;
    }

    private sealed class AppleAuthDelegate(TaskCompletionSource<AuthenticationServices.ASAuthorization?> tcs)
        : Foundation.NSObject,
          AuthenticationServices.IASAuthorizationControllerDelegate,
          AuthenticationServices.IASAuthorizationControllerPresentationContextProviding
    {
        [Foundation.Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(
            AuthenticationServices.ASAuthorizationController controller,
            AuthenticationServices.ASAuthorization authorization)
        {
            tcs.TrySetResult(authorization);
        }

        [Foundation.Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(
            AuthenticationServices.ASAuthorizationController controller,
            Foundation.NSError error)
        {
            tcs.TrySetResult(null);
        }

        public UIKit.UIWindow GetPresentationAnchor(AuthenticationServices.ASAuthorizationController controller)
            => Microsoft.Maui.ApplicationModel.Platform.GetCurrentUIViewController()?.View?.Window
               ?? UIKit.UIApplication.SharedApplication.KeyWindow
               ?? new UIKit.UIWindow();
    }
#endif
}
