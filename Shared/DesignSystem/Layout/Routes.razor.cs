namespace Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Domain.Interfaces;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

public partial class Routes : ComponentBase, IDisposable
{
    private bool _subscribedToFileNavigation;
    private string? _lastFileHashPath;
    private string? _fileOverrideRoute;
    private DotNetObjectReference<Routes>? _selfRef;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (Navigation.Uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            _lastFileHashPath = ExtractFileHashPath(Navigation.Uri);
            Navigation.LocationChanged += OnFileLocationChanged;
            _subscribedToFileNavigation = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _subscribedToFileNavigation)
        {
            try
            {
                _selfRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("__registerFileRouter", _selfRef);
            }
            catch
            {
                // Ignore if standalone helper is not present
            }
        }
    }

    [JSInvokable]
    public Task NavigateFileRouteAsync(string route)
    {
        var qIdx = route.IndexOf('?');
        var cleanPath = qIdx >= 0 ? route[..qIdx] : route;
        if (string.IsNullOrEmpty(cleanPath)) cleanPath = "/";
        _fileOverrideRoute = cleanPath;
        _lastFileHashPath = cleanPath;
        return InvokeAsync(StateHasChanged);
    }

    private void OnFileLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var nextPath = ExtractFileHashPath(e.Location);
        if (string.Equals(_lastFileHashPath, nextPath, StringComparison.OrdinalIgnoreCase))
            return;

        _fileOverrideRoute = nextPath;
        _lastFileHashPath = nextPath;
        InvokeAsync(StateHasChanged);
    }

    private static string ExtractFileHashPath(string uri)
    {
        var hashIdx = uri.IndexOf('#');
        var hashRoute = hashIdx >= 0 ? uri[(hashIdx + 1)..] : "/";
        var qIdx = hashRoute.IndexOf('?');
        if (qIdx >= 0)
            hashRoute = hashRoute[..qIdx];
        return string.IsNullOrEmpty(hashRoute) ? "/" : hashRoute;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Used only for standalone file:// hash-routing preview.")]
    protected bool TryResolveFileHashRoute([NotNullWhen(true)] out RouteData? routeData)
    {
        routeData = null;
        if (!Navigation.Uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return false;

        var hashRoute = _fileOverrideRoute ?? ExtractFileHashPath(Navigation.Uri);
        if (!hashRoute.StartsWith('/'))
            return false;

        var actualSegments = hashRoute.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var assemblies = FeatureRegistry.GetFeatureAssemblies()
            .Append(typeof(Layout.AppLayout).Assembly)
            .Distinct();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetExportedTypes())
            {
                if (type.IsAbstract || !typeof(IComponent).IsAssignableFrom(type))
                    continue;

                foreach (var attr in type.GetCustomAttributes<RouteAttribute>(inherit: false))
                {
                    if (TryMatchTemplate(attr.Template, actualSegments, out var routeValues))
                    {
                        routeData = new RouteData(type, routeValues);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool TryMatchTemplate(
        string template,
        string[] actualSegments,
        out IReadOnlyDictionary<string, object?> routeValues)
    {
        var templateSegments = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (templateSegments.Length == 0 && actualSegments.Length == 0)
        {
            routeValues = new Dictionary<string, object?>();
            return true;
        }

        if (actualSegments.Length > templateSegments.Length)
        {
            routeValues = null!;
            return false;
        }

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < templateSegments.Length; i++)
        {
            var seg = templateSegments[i];
            if (seg.StartsWith('{') && seg.EndsWith('}'))
            {
                var inner = seg[1..^1];
                var isOptional = inner.EndsWith('?');
                if (isOptional) inner = inner[..^1];
                var colonIdx = inner.IndexOf(':');
                var paramName = colonIdx >= 0 ? inner[..colonIdx] : inner;

                if (i < actualSegments.Length)
                {
                    values[paramName] = Uri.UnescapeDataString(actualSegments[i]);
                }
                else if (!isOptional)
                {
                    routeValues = null!;
                    return false;
                }
            }
            else
            {
                if (i >= actualSegments.Length || !string.Equals(seg, actualSegments[i], StringComparison.OrdinalIgnoreCase))
                {
                    routeValues = null!;
                    return false;
                }
            }
        }

        routeValues = values;
        return true;
    }

    public void Dispose()
    {
        if (_subscribedToFileNavigation)
            Navigation.LocationChanged -= OnFileLocationChanged;
        _selfRef?.Dispose();
    }
}
