using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class Fab : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string Size { get; set; } = "medium"; // small, medium, large
    [Parameter] public string Variant { get; set; } = "surface"; // surface, primary, secondary, tertiary
    [Parameter] public bool Lowered { get; set; } = false;
    [Parameter] public bool Branded { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
