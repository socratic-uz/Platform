using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Shared.Layout
{
    public partial class Mode : ComponentBase
    {
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}
