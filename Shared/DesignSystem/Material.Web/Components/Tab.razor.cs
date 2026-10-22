using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Tab : ComponentBase
{
    [CascadingParameter] public Tabs? Parent { get; set; }

    [Parameter] public int Index { get; set; }
    [Parameter] public bool InlineIcon { get; set; } = false;
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool IsSecondary { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool IsActive => Parent != null && Parent.ActiveTabIndex == Index;

    protected async Task HandleClick()
    {
        if (Parent != null)
        {
            await Parent.SelectTab(Index);
        }
    }
}
