namespace Shared.Layout;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Shared.Extensions;
using Shared.Services;

public partial class Nav : ComponentBase
{
    public const string Name = "nav";

    [Inject] public IStringLocalizerFactory F { get; set; } = default!;
    [Inject] public ISettingsManager Settings { get; set; } = default!;

    protected IStringLocalizer? L { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool Default { get; set; }
    [Parameter] public bool Empty { get; set; }
    [Parameter] public bool Visible { get; set; } = true;

    protected bool ShouldRenderCustom => ChildContent != null && !Default;

    protected override async Task OnInitializedAsync()
    {
        L = await F.Create(Settings);
        StateHasChanged();
    }
}

