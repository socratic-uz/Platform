using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components.Labs;

public partial class Card : ComponentBase
{
    [Parameter] public string Type { get; set; } = "elevated"; // elevated, filled, outlined
    [Parameter] public bool Expressive { get; set; } = true;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string ComputedCssClass => $"{(Expressive ? "expressive" : "")} {GetCustomClass()}".Trim();

    protected string GetCustomClass()
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
}
