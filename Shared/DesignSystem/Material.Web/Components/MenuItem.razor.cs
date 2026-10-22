using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class MenuItem : ComponentBase
{
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public string Type { get; set; } = "menuitem";
    [Parameter] public string Href { get; set; } = "";
    [Parameter] public string Target { get; set; } = "";
    [Parameter] public bool KeepOpen { get; set; } = false;
    [Parameter] public bool Selected { get; set; } = false;
    
    [Parameter] public string? Headline { get; set; }
    [Parameter] public string? SupportingText { get; set; }
    [Parameter] public string? StartIcon { get; set; }
    [Parameter] public string? EndIcon { get; set; }
    
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (!Disabled)
        {
            await OnClick.InvokeAsync(e);
        }
    }
}
