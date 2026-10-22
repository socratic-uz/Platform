using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class List : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
