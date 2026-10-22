using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class NavigationTab : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Active { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool HideInactiveLabel { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
