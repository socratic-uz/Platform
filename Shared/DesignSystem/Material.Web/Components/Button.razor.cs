using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Material.Web.Components;

public partial class Button : ComponentBase
{
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;
    [Parameter] public bool Expressive { get; set; } = true;
    [Parameter] public bool Pill { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public string? Type { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? TrailingIcon { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string ComputedCssClass =>
        $"{(Expressive ? "expressive" : "")} {(Pill ? "pill" : "")} {GetCustomClass()}".Trim();

    private string GetCustomClass()
    {
        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var cls) && cls != null)
        {
            return cls.ToString() ?? "";
        }
        return "";
    }

    protected IDictionary<string, object>? FilteredAttributes
    {
        get
        {
            if (AdditionalAttributes == null) return null;
            var copy = new Dictionary<string, object>(AdditionalAttributes);
            copy.Remove("class");
            return copy;
        }
    }

    protected RenderFragment RenderButtonContent() => builder =>
    {
        if (!string.IsNullOrEmpty(Icon))
        {
            builder.OpenElement(0, "md-icon");
            builder.AddAttribute(1, "slot", "icon");
            builder.AddContent(2, Icon);
            builder.CloseElement();
        }

        if (ChildContent != null)
        {
            builder.AddContent(3, ChildContent);
        }

        if (!string.IsNullOrEmpty(TrailingIcon))
        {
            builder.OpenElement(4, "md-icon");
            builder.AddAttribute(5, "slot", "trailing-icon");
            builder.AddContent(6, TrailingIcon);
            builder.CloseElement();
        }
    };
}
