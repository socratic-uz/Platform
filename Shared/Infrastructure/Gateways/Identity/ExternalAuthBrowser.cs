using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Gateways.Identity;

public sealed record ExternalAuthResult(IReadOnlyDictionary<string, string> Properties);

public interface IExternalAuthBrowser
{
    bool IsSupported { get; }
    string PublicOrigin { get; }
    Task<ExternalAuthResult?> AuthenticateAsync(Uri authorizeUrl, Uri callbackUrl);
}

public sealed class DefaultExternalAuthBrowser : IExternalAuthBrowser
{
    public bool IsSupported => false;
    public string PublicOrigin => string.Empty;

    public Task<ExternalAuthResult?> AuthenticateAsync(Uri authorizeUrl, Uri callbackUrl)
        => Task.FromResult<ExternalAuthResult?>(null);
}
