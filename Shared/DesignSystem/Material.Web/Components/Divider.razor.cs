using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Divider : ComponentBase
{
    [Parameter] public bool Inset { get; set; } = false;
    [Parameter] public bool Vertical { get; set; } = false;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
