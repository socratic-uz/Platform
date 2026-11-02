namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using Shared.Extensions;
using Shared.Services;
using static Assets.Constants.Classes.Common;
using static Assets.Constants.Formats;

/// <summary>
/// Renders the semantic &lt;nav&gt; element with standard design system classes
/// and content provided by <see cref="Nav"/> components.
/// </summary>
public sealed class NavOutlet : ComponentBase
{
    [Inject]
    private IFormFactor FormFactor { get; set; } = default!;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string AriaLabel { get; set; } = "Main Navigation";

    private string ComputedClass =>
        string.IsNullOrEmpty(Class)
            ? string.Format(Two, Glass, FormFactor.IsMobile() ? NavMobile : NavDesktop)
            : string.Format(Three, Glass, FormFactor.IsMobile() ? NavMobile : NavDesktop, Class);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", ComputedClass);
        builder.AddAttribute(2, "aria-label", AriaLabel);
        builder.OpenComponent<SectionOutlet>(3);
        builder.AddAttribute(4, nameof(SectionOutlet.SectionName), Nav.Name);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
