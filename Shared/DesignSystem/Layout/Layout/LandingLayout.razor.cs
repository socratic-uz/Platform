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

        public const string LandingFloatingControlsClass = "landing-floating-controls";
        public const string LandingFloatingAuthBtnClass = "landing-floating-auth-btn";
        public const string AuthBtnLabelClass = "auth-btn-label";
        public const string LandingHeaderClass = "landing-header";
        public const string LandingHeaderNavClass = "landing-header-nav";
        public const string LandingNavLinkClass = "landing-nav-link";
        public const string LandingHeaderActionsClass = "landing-header-actions";
        public const string LandingAuthBtnClass = "landing-auth-btn";
        public const string LandingMobileMenuToggleClass = "landing-mobile-menu-toggle";
        public const string LandingMobileMenuBackdropClass = "landing-mobile-menu-backdrop";
        public const string LandingMobileMenuDrawerClass = "landing-mobile-menu-drawer";
        public const string MobileNavItemClass = "mobile-nav-item";
        public const string MobileNavDividerClass = "mobile-nav-divider";
        public const string MobileNavFooterClass = "mobile-nav-footer";

        public ErrorBoundary? ErrorBoundaryRef { get; set; }
        public bool IsMobileMenuOpen { get; set; }
        public bool IsRootLanding => Nav == null || string.IsNullOrEmpty(Nav.ToBaseRelativePath(Nav.Uri).Trim('/'));

        public string MenuIcon => IsMobileMenuOpen ? Icons.Close : Icons.Menu;

        public void ToggleMobileMenu() => IsMobileMenuOpen = !IsMobileMenuOpen;
        public void CloseMobileMenu() => IsMobileMenuOpen = false;
        public void Recover() => ErrorBoundaryRef?.Recover();
    }
}
