using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components.Labs;

public partial class SegmentedButton : ComponentBase
{
    [Parameter] public bool Selected { get; set; } = false;
    [Parameter] public EventCallback<bool> SelectedChanged { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (Disabled) return;
        Selected = !Selected;
        await SelectedChanged.InvokeAsync(Selected);
        await OnClick.InvokeAsync(e);
    }
}
