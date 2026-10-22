using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Assets.Constants;

namespace Shared.Layout
{
    public partial class LandingLayout : LayoutComponentBase
    {
        [Inject] public NavigationManager? Nav { get; set; }
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public ErrorBoundary? ErrorBoundaryRef { get; set; }
        public bool IsMobileMenuOpen { get; set; }
        public bool IsRootLanding => Nav == null || string.IsNullOrEmpty(Nav.ToBaseRelativePath(Nav.Uri).Trim('/'));

        public string MenuIcon => IsMobileMenuOpen ? Icons.Close : Icons.Menu;

        public void ToggleMobileMenu() => IsMobileMenuOpen = !IsMobileMenuOpen;
        public void CloseMobileMenu() => IsMobileMenuOpen = false;
        public void Recover() => ErrorBoundaryRef?.Recover();

        public string T(string key, string fallback)
        {
            if (L == null) return fallback;
            try
            {
                var localized = L[key];
                if (localized == null || localized.ResourceNotFound || string.IsNullOrWhiteSpace(localized.Value) || localized.Value == key)
                {
                    return fallback;
                }
                return localized.Value;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
