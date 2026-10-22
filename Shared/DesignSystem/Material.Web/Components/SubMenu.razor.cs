using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class SubMenu : ComponentBase
{
    [Parameter] public string AnchorCorner { get; set; } = "start-end";
    [Parameter] public string MenuCorner { get; set; } = "start-start";
    [Parameter] public int HoverOpenDelay { get; set; } = 400;
    [Parameter] public int HoverCloseDelay { get; set; } = 400;
    
    // Item properties
    [Parameter] public bool ItemDisabled { get; set; } = false;
    [Parameter] public bool ItemSelected { get; set; } = false;
    [Parameter] public string? ItemHeadline { get; set; }
    [Parameter] public string? ItemSupportingText { get; set; }
    [Parameter] public string? ItemStartIcon { get; set; }
    [Parameter] public string? ItemEndIcon { get; set; }
    [Parameter] public RenderFragment? ItemContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnItemClick { get; set; }
    
    // Menu content
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Parameter] public Dictionary<string, object> MenuAttributes { get; set; } = new();
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
}
