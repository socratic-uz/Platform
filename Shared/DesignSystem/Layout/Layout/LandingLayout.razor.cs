using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace Shared.Layout;

public enum NavDropdownGroup
{
    None,
    Features,
    Catalog,
    Resources,
    Community
}

public partial class LandingLayout : LayoutComponentBase
{
    [Inject] public NavigationManager? Nav { get; set; }
    [Inject] public IStringLocalizer L { get; set; } = default!;

    public ErrorBoundary? ErrorBoundaryRef { get; set; }
    public bool IsMobileMenuOpen { get; set; }
    public bool IsRootLanding => Nav == null || string.IsNullOrEmpty(Nav.ToBaseRelativePath(Nav.Uri).Trim('/'));

    public NavDropdownGroup ActiveDropdown { get; set; } = NavDropdownGroup.None;

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
