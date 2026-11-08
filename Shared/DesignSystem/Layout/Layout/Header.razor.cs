namespace Shared.Layout;

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public partial class Header : ComponentBase
{
    public const string Name = "header";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
}
