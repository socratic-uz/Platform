using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class ListItem : ComponentBase
{
    [Parameter] public string? Headline { get; set; }
    [Parameter] public string? SupportingText { get; set; }
    [Parameter] public bool MultiLineSupportingText { get; set; } = false;
    [Parameter] public string? TrailingSupportingText { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Selected { get; set; } = false;
    [Parameter] public EventCallback<bool> SelectedChanged { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public RenderFragment? StartContent { get; set; }
    [Parameter] public RenderFragment? EndContent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Type { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string EffectiveType
    {
        get
        {
            if (!string.IsNullOrEmpty(Type)) return Type;
            if (!string.IsNullOrEmpty(Href)) return "link";
            if (OnClick.HasDelegate) return "button";
            return "text";
        }
    }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (Disabled) return;
        if (SelectedChanged.HasDelegate)
        {
            Selected = !Selected;
            await SelectedChanged.InvokeAsync(Selected);
        }
        await OnClick.InvokeAsync(e);
    }
}
