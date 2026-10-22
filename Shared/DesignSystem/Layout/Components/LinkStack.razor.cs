using Microsoft.AspNetCore.Components;

namespace Shared.Layout
{
    public partial class LinkStack : ComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
    }
}
