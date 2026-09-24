using System;
using Microsoft.JSInterop;

namespace Shared.Services
{
    [Obsolete("Use Infrastructure.Gateways.Identity.IPasskeyService instead.")]
    public interface IPasskeyService : Infrastructure.Gateways.Identity.IPasskeyService
    {
    }

    [Obsolete("Use Infrastructure.Gateways.Identity.PasskeyService instead.")]
    public class PasskeyService : Infrastructure.Gateways.Identity.PasskeyService, IPasskeyService
    {
        public PasskeyService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
    }
}
