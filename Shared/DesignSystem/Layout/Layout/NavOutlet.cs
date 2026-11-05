namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using Shared.Extensions;
using Shared.Services;

/// <summary>
/// Renders the semantic &lt;nav&gt; element with landmark attributes
/// and content provided by <see cref="Nav"/> components.
/// </summary>
public sealed class NavOutlet : ComponentBase
{
    [Inject]
    private IFormFactor FormFactor { get; set; } = default!;

    [Parameter]
    public string AriaLabel { get; set; } = "Main Navigation";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "aria-label", AriaLabel);
        if (FormFactor.IsMobile())
        {
            builder.AddAttribute(2, "data-mobile", "true");
        }
        builder.OpenComponent<SectionOutlet>(3);
        builder.AddAttribute(4, nameof(SectionOutlet.SectionName), Nav.Name);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
