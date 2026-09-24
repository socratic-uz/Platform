using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    [Obsolete("Use Infrastructure.Gateways.Biometrics.ClientFaceIdService instead.")]
    public class ClientFaceIdService : Infrastructure.Gateways.Biometrics.ClientFaceIdService
    {
        public ClientFaceIdService(HttpClient? httpClient = null, ILogger<ClientFaceIdService>? logger = null)
            : base(httpClient, null)
        {
        }
    }
}
