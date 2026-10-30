// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Markdown.Web;

internal class ServerStaticAssetService : IStaticAssetService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public ServerStaticAssetService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        try
        {
            if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(navigationManager.BaseUri))
            {
                _httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
            }
        }
        catch (InvalidOperationException)
        {
            // BaseAddress cannot be modified once requests have started on this HttpClient instance
        }
    }

    public async Task<string?> GetAsync(string assetUrl, bool useCache = true)
    {
        Uri requestUri = Uri.TryCreate(assetUrl, UriKind.Absolute, out var abs) 
            ? abs 
            : new Uri(new Uri(_navigationManager.BaseUri), assetUrl);

        var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await _httpClient.SendAsync(message);

        return await response.Content.ReadAsStringAsync();
    }
}
