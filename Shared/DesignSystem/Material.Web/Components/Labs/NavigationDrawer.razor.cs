using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class NavigationDrawer : ComponentBase
{
    [Parameter] public bool Opened { get; set; } = false;
    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }
    [Parameter] public string Pivot { get; set; } = "start"; // start, end
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClosed(EventArgs e)
    {
        Opened = false;
        await OpenedChanged.InvokeAsync(false);
    }
}
