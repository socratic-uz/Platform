using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

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
    }
}
