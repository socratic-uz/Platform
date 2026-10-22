using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Progress : ComponentBase
{
    [Parameter] public bool IsCircular { get; set; } = false;
    [Parameter] public double? Value { get; set; }
    [Parameter] public double? Buffer { get; set; }
    [Parameter] public bool Indeterminate { get; set; } = true;
    [Parameter] public bool FourColor { get; set; } = false;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool IsIndeterminate => Indeterminate && !Value.HasValue;
}
