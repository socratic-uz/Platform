using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Shared.Extensions;
using Shared.Services;

namespace Shared.Layout
{
    public partial class Nav : ComponentBase
    {
        [Inject] public IFormFactor FormFactor { get; set; } = default!;
        [Inject] public IStringLocalizerFactory F { get; set; } = default!;
        [Inject] public ISettingsManager Settings { get; set; } = default!;

        protected IStringLocalizer? L { get; set; }

        [Parameter] public RenderFragment? ChildContent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            L = await F.Create(Settings);
            StateHasChanged();
        }
    }
}
