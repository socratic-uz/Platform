namespace Shared.Layout;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Shared.Extensions;
using Shared.Services;

public partial class Aside : ComponentBase
{
    public const string Name = "aside";
    public const string SecondaryName = "aside-secondary";

    [Inject] public IStringLocalizerFactory F { get; set; } = default!;
    [Inject] public ISettingsManager Settings { get; set; } = default!;

    protected IStringLocalizer? L { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? NavContent { get; set; }
    [Parameter] public RenderFragment? DialogContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnToggleDialog { get; set; }
    [Parameter] public bool NoNav { get; set; }
    [Parameter] public bool Secondary { get; set; }
    [Parameter] public bool Default { get; set; }
    [Parameter] public bool Empty { get; set; }
    [Parameter] public bool Visible { get; set; } = true;

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool ShouldRenderCustomNav => NavContent != null || (ChildContent != null && !Default && DialogContent == null);

    protected override async Task OnInitializedAsync()
    {
        L = await F.Create(Settings);
        StateHasChanged();
    }
}
