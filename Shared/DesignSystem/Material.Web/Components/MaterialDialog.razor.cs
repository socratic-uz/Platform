using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class MaterialDialog : ComponentBase
{
    [Parameter] public bool Open { get; set; } = false;
    [Parameter] public string Type { get; set; } = "standard"; // standard, alert
    [Parameter] public bool NoFocusTrap { get; set; } = false;
    [Parameter] public RenderFragment? Headline { get; set; }
    [Parameter] public RenderFragment? Content { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<EventArgs> OnClose { get; set; }
    [Parameter] public EventCallback<EventArgs> OnCancel { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClose(EventArgs e)
    {
        Open = false;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync(e);
    }

    protected async Task HandleCancel(EventArgs e)
    {
        Open = false;
        await OpenChanged.InvokeAsync(false);
        await OnCancel.InvokeAsync(e);
    }
}
