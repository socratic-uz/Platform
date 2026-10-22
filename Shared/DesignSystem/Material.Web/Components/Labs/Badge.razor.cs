using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class Badge : ComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
