using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class Footer : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public const string FooterBottomBarClass = "footer-bottom-bar";
        public const string FooterBrandBadgeClass = "footer-brand-badge";
        public const string StatusPulseDotClass = "status-pulse-dot";
        public const string StatusTextClass = "status-text";
        public const string FooterTrustInfoClass = "footer-trust-info";
        public const string TrustBadgeClass = "trust-badge";

        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string Class { get; set; } = string.Empty;
    }
}
