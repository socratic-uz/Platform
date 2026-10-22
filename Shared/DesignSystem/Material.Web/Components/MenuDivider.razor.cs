using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class MenuDivider : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
}
