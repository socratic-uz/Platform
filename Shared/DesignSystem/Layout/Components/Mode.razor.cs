using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class Mode : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}
