using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Infrastructure.Gateways.Identity
{
    public interface IPasskeyService
    {
        ValueTask<bool> IsSupportedAsync();
        ValueTask<string?> RegisterAsync(string userId, string userName, string challengeBase64);
        ValueTask<string?> AuthenticateAsync(string challengeBase64);
    }

    public class PasskeyService : IPasskeyService
    {
        private readonly IJSRuntime _jsRuntime;

        public PasskeyService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<bool> IsSupportedAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("passkeyHelper.isSupported");
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask<string?> RegisterAsync(string userId, string userName, string challengeBase64)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("passkeyHelper.register", userId, userName, challengeBase64);
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask<string?> AuthenticateAsync(string challengeBase64)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("passkeyHelper.authenticate", challengeBase64);
            }
            catch
            {
                return null;
            }
        }
    }
}
