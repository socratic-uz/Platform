using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using static Assets.Constants.Classes.Header;
using static Assets.Constants.Formats;

namespace Shared.Layout
{
    public partial class Header : ComponentBase
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IStringLocalizer L { get; set; } = default!;

        [Parameter] public RenderFragment? StartContent { get; set; }
        [Parameter] public RenderFragment? CenterContent { get; set; }
        [Parameter] public RenderFragment? EndContent { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public bool ShowBrand { get; set; } = true;
        [Parameter] public bool ShowDefaultActions { get; set; } = true;
        [Parameter] public bool ShowAvatar { get; set; } = false;
        [Parameter] public bool Sticky { get; set; } = true;
        [Parameter] public string Class { get; set; } = string.Empty;

        protected string ComputedHeaderClass =>
            Sticky
                ? (string.IsNullOrEmpty(Class)
                    ? string.Format(Two, AppHeader, AppHeaderSticky)
                    : string.Format(Three, AppHeader, AppHeaderSticky, Class))
                : (string.IsNullOrEmpty(Class)
                    ? AppHeader
                    : string.Format(Two, AppHeader, Class));
    }
}
