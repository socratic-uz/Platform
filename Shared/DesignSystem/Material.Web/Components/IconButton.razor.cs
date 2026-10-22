using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class IconButton : ComponentBase
{
    [Parameter] public IconButtonVariant Variant { get; set; } = IconButtonVariant.Standard;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public string? Type { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public bool Toggle { get; set; } = false;
    [Parameter] public bool Selected { get; set; } = false;
    [Parameter] public EventCallback<bool> SelectedChanged { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (Disabled) return;
        if (Toggle)
        {
            Selected = !Selected;
            await SelectedChanged.InvokeAsync(Selected);
        }
        await OnClick.InvokeAsync(e);
    }
}
