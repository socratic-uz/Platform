namespace Shared.Layout;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;

public enum NavDropdownGroup
{
    None,
    Features,
    Catalog,
    Resources,
    Community
}

public partial class LandingHeader : ComponentBase, IDisposable
{
    [Inject] public NavigationManager? Nav { get; set; }
    [Inject] public IStringLocalizer L { get; set; } = default!;

    public bool IsMobileMenuOpen { get; set; }
    public NavDropdownGroup ActiveDropdown { get; set; } = NavDropdownGroup.None;
    private CancellationTokenSource? _hoverCloseCts;

    protected override void OnInitialized()
    {
        if (Nav != null)
        {
            Nav.LocationChanged += OnLocationChanged;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        IsMobileMenuOpen = false;
        ActiveDropdown = NavDropdownGroup.None;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _hoverCloseCts?.Cancel();
        _hoverCloseCts?.Dispose();
        if (Nav != null)
        {
            Nav.LocationChanged -= OnLocationChanged;
        }
    }

    public void OpenDropdown(NavDropdownGroup group)
    {
        _hoverCloseCts?.Cancel();
        ActiveDropdown = group;
    }

    public async Task ScheduleCloseDropdownAsync(NavDropdownGroup group)
    {
        _hoverCloseCts?.Cancel();
        _hoverCloseCts?.Dispose();
        _hoverCloseCts = new CancellationTokenSource();
        var token = _hoverCloseCts.Token;

        try
        {
            await Task.Delay(120, token);
            if (!token.IsCancellationRequested && ActiveDropdown == group)
            {
                ActiveDropdown = NavDropdownGroup.None;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    public void ToggleDropdown(NavDropdownGroup group)
    {
        _hoverCloseCts?.Cancel();
        ActiveDropdown = ActiveDropdown == group ? NavDropdownGroup.None : group;
    }

    public void CloseDropdown()
    {
        _hoverCloseCts?.Cancel();
        ActiveDropdown = NavDropdownGroup.None;
    }

    public void ToggleMobileMenuAsync() => IsMobileMenuOpen = !IsMobileMenuOpen;
    public void CloseMobileMenuAsync() => IsMobileMenuOpen = false;

    public string? GetAriaCurrent(string href) => IsCurrent(href) ? "page" : null;

    public bool IsCurrent(string href)
    {
        if (Nav == null) return false;
        var relative = "/" + Nav.ToBaseRelativePath(Nav.Uri).Trim('/');
        if (href == "/")
            return relative == "/" || relative.StartsWith("/?");
        return relative.Equals(href, StringComparison.OrdinalIgnoreCase) || relative.StartsWith(href + "/", StringComparison.OrdinalIgnoreCase);
    }
}

