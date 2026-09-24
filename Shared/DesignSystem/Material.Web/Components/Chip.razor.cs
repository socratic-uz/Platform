using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

/// <summary>
/// Material Design 3 Chip component according to official documentation
/// Supports all four chip types: Assist, Filter, Input, and Suggestion
/// </summary>
public partial class Chip
{
    // Core MD3 Chip properties
    [Parameter] public ChipType ChipType { get; set; } = ChipType.Filter;
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public bool Selected { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Elevated { get; set; } = false;
    [Parameter] public bool Expressive { get; set; } = true;
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    protected string ComputedCssClass => $"{(Expressive ? "expressive" : "")} {CssClass}".Trim();
    
    // Icon properties
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? SelectedIcon { get; set; } = "check";
    [Parameter] public bool ShowSelectedIcon { get; set; } = true;
    
    // Filter chip specific
    [Parameter] public bool Removable { get; set; } = false;
    
    // Input chip specific
    [Parameter] public bool RemoveOnly { get; set; } = false;
    [Parameter] public bool Avatar { get; set; } = false;
    
    // Assist/Suggestion chip specific
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    
    // Events according to MD3 specification
    [Parameter] public EventCallback<bool> SelectedChanged { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }
    
    // Content and additional attributes
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private async Task HandleClick(MouseEventArgs e)
    {
        if (Disabled) return;
        Selected = !Selected;
        await SelectedChanged.InvokeAsync(Selected);
        await OnClick.InvokeAsync(e);
    }

}