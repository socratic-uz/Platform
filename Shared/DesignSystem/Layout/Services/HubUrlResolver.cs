using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Shared.Services;

[Obsolete("Use Infrastructure.Gateways.Vision.HubUrlResolver instead.")]
public static class HubUrlResolver
{
    public static string ResolveHubUrl(NavigationManager? navManager, IConfiguration? configuration, string relativePath)
        => Infrastructure.Gateways.Vision.HubUrlResolver.ResolveHubUrl(navManager, configuration, relativePath);
}
