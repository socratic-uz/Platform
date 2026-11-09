namespace Shared.Layout;

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public partial class Footer : ComponentBase
{
    public const string Name = "footer";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
}
