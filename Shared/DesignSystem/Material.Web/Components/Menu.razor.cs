using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Menu : ComponentBase
{
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public string Anchor { get; set; } = "";
    [Parameter] public string Positioning { get; set; } = "absolute";
    [Parameter] public bool Quick { get; set; } = false;
    [Parameter] public bool HasOverflow { get; set; } = false;
    [Parameter] public bool Open { get; set; } = false;
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public int XOffset { get; set; } = 0;
    [Parameter] public int YOffset { get; set; } = 0;
    [Parameter] public bool NoHorizontalFlip { get; set; } = false;
    [Parameter] public bool NoVerticalFlip { get; set; } = false;
    [Parameter] public int TypeaheadDelay { get; set; } = 200;
    [Parameter] public string AnchorCorner { get; set; } = "end-start";
    [Parameter] public string MenuCorner { get; set; } = "start-start";
    [Parameter] public bool StayOpenOnOutsideClick { get; set; } = false;
    [Parameter] public bool StayOpenOnFocusout { get; set; } = false;
    [Parameter] public bool SkipRestoreFocus { get; set; } = false;
    [Parameter] public string DefaultFocus { get; set; } = "first-item";
    [Parameter] public bool NoNavigationWrap { get; set; } = false;
    
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Parameter] public EventCallback<EventArgs> OnOpening { get; set; }
    [Parameter] public EventCallback<EventArgs> OnOpened { get; set; }
    [Parameter] public EventCallback<EventArgs> OnClosing { get; set; }
    [Parameter] public EventCallback<EventArgs> OnClosed { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    protected async Task HandleClosed(EventArgs e)
    {
        Open = false;
        await OpenChanged.InvokeAsync(false);
        await OnClosed.InvokeAsync(e);
    }

    protected async Task HandleClosing(EventArgs e)
    {
        await OnClosing.InvokeAsync(e);
    }

    protected async Task HandleOpened(EventArgs e)
    {
        Open = true;
        await OpenChanged.InvokeAsync(true);
        await OnOpened.InvokeAsync(e);
    }

    protected async Task HandleOpening(EventArgs e)
    {
        await OnOpening.InvokeAsync(e);
    }
}
