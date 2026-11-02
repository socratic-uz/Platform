namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

public partial class Header : ComponentBase
{
    public const string Name = "header";

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
    [Parameter] public bool Default { get; set; }
    [Parameter] public bool Empty { get; set; }
    [Parameter] public bool Visible { get; set; } = true;

    protected bool ShouldRenderCustom => ChildContent != null && !Default;
}

