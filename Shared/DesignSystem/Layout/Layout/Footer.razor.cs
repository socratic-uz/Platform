namespace Shared.Layout;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

public partial class Footer : ComponentBase
{
    public const string Name = "footer";

    [Inject] public IStringLocalizer L { get; set; } = default!;

    public static IReadOnlyDictionary<string, object>? CurrentAttributes { get; private set; }
    public static event Action? OnFooterChanged;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool Default { get; set; }
    [Parameter] public bool Empty { get; set; }
    [Parameter] public bool Visible { get; set; } = true;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool ShouldRenderCustom => ChildContent != null && !Default;

    protected override void OnParametersSet()
    {
        CurrentAttributes = AdditionalAttributes;
        OnFooterChanged?.Invoke();
    }
}
