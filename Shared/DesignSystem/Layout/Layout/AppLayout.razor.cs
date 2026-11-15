namespace Shared.Layout;

using System;
using System.Diagnostics.CodeAnalysis;
using Domain.Interfaces.Map;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

public partial class AppLayout : LayoutComponentBase
{
    [Inject] public IStringLocalizer L { get; set; } = default!;
    [Inject] public IServiceProvider ServiceProvider { get; set; } = default!;

    public ErrorBoundary? ErrorBoundaryRef { get; set; }

    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "MapPopupHost type is annotated with DynamicallyAccessedMembers(All) on IMapUiProvider")]
    private Type? MapPopupHostType =>
        (ServiceProvider.GetService(typeof(IMapUiProvider)) as IMapUiProvider)
        ?.GetMapPopupHostComponentType();

    public void Recover() => ErrorBoundaryRef?.Recover();
}
