using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class SegmentedButtonSet : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
