using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Icon : ComponentBase
{
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Slot { get; set; }
    [Parameter] public string? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected string IconText
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Name)) return Name;
            if (!string.IsNullOrWhiteSpace(ChildContent)) return ChildContent;
            return string.Empty;
        }
    }

    protected string? EffectiveSlot
    {
        get
        {
            if (!string.IsNullOrEmpty(Slot)) return Slot;
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("slot", out var slotObj) && slotObj != null)
            {
                return slotObj.ToString();
            }
            return null;
        }
    }

    protected IDictionary<string, object>? UnmatchedAttributes
    {
        get
        {
            if (AdditionalAttributes == null) return null;
            var copy = new Dictionary<string, object>(AdditionalAttributes);
            copy.Remove("slot");
            copy.Remove("name");
            return copy;
        }
    }
}
