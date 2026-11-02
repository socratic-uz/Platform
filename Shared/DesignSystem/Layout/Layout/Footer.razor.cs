namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

public partial class Footer : ComponentBase
{
    public const string Name = "footer";

    [Inject] public IStringLocalizer L { get; set; } = default!;

    public const string FooterBottomBarClass = "footer-bottom-bar";
    public const string FooterBrandBadgeClass = "footer-brand-badge";
    public const string StatusPulseDotClass = "status-pulse-dot";
    public const string StatusTextClass = "status-text";
    public const string FooterTrustInfoClass = "footer-trust-info";
    public const string TrustBadgeClass = "trust-badge";

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool Default { get; set; }
    [Parameter] public bool Empty { get; set; }
    [Parameter] public bool Visible { get; set; } = true;

    protected bool ShouldRenderCustom => ChildContent != null && !Default;
}

