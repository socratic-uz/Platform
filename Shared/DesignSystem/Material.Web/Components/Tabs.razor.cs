using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Tabs : ComponentBase
{
    [Parameter] public int ActiveTabIndex { get; set; } = 0;
    [Parameter] public EventCallback<int> ActiveTabIndexChanged { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    internal async Task SelectTab(int index)
    {
        if (index != ActiveTabIndex)
        {
            ActiveTabIndex = index;
            await ActiveTabIndexChanged.InvokeAsync(index);
        }
    }

    protected async Task HandleTabChange(ChangeEventArgs e)
    {
        if (e.Value != null && int.TryParse(e.Value.ToString(), out var index))
        {
            await SelectTab(index);
        }
    }
}
