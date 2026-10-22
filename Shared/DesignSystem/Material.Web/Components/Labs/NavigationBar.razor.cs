using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class NavigationBar : ComponentBase
{
    [Parameter] public int ActiveIndex { get; set; } = 0;
    [Parameter] public bool HideInactiveLabels { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
