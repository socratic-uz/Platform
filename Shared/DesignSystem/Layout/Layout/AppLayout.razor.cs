namespace Shared.Layout;

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

public enum LayoutMode
{
    App,
    Landing,
    Chat
}

public enum NavDropdownGroup
{
    None,
    Features,
    Catalog,
    Resources,
    Community
}

public partial class AppLayout : LayoutComponentBase, IDisposable
{
    [Inject] public NavigationManager? Nav { get; set; }
    [Inject] public IStringLocalizer L { get; set; } = default!;

    public ErrorBoundary? ErrorBoundaryRef { get; set; }
    public bool IsMobileMenuOpen { get; set; }
    public NavDropdownGroup ActiveDropdown { get; set; } = NavDropdownGroup.None;

    public bool IsRootLanding => Nav == null || string.IsNullOrEmpty(Nav.ToBaseRelativePath(Nav.Uri).Trim('/'));

    public LayoutMode Mode
    {
        get
        {
            if (Nav == null) return LayoutMode.App;
            var relative = Nav.ToBaseRelativePath(Nav.Uri).Trim('/');
            var path = relative.Split('?')[0].ToLowerInvariant();

            if (string.IsNullOrEmpty(path) || path == "about" || path == "help")
                return LayoutMode.Landing;

            if (path == "chat" || path.StartsWith("chat/"))
                return LayoutMode.Chat;

            return LayoutMode.App;
        }
    }

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
        if (Nav != null)
        {
            Nav.LocationChanged -= OnLocationChanged;
        }
    }

    public void ToggleDropdown(NavDropdownGroup group)
    {
        ActiveDropdown = ActiveDropdown == group ? NavDropdownGroup.None : group;
    }

    public void CloseDropdown()
    {
        ActiveDropdown = NavDropdownGroup.None;
    }

    public void ToggleMobileMenuAsync() => IsMobileMenuOpen = !IsMobileMenuOpen;
    public void CloseMobileMenuAsync() => IsMobileMenuOpen = false;
    public void Recover() => ErrorBoundaryRef?.Recover();

    public string? GetAriaCurrent(string href) => IsCurrent(href) ? "page" : null;

    public bool IsCurrent(string href)
    {
        if (Nav == null) return false;
        var relative = "/" + Nav.ToBaseRelativePath(Nav.Uri).Trim('/');
        if (href == "/")
            return relative == "/" || relative.StartsWith("/?");
        return relative.Equals(href, StringComparison.OrdinalIgnoreCase) || relative.StartsWith(href + "/", StringComparison.OrdinalIgnoreCase);
    }

    public void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            if (ActiveDropdown != NavDropdownGroup.None)
            {
                CloseDropdown();
            }
            if (IsMobileMenuOpen)
            {
                IsMobileMenuOpen = false;
            }
        }
    }
}
