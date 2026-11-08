namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using Shared.Extensions;
using Shared.Services;

/// <summary>
/// Renders the semantic &lt;aside&gt; rail element wrapping &lt;nav&gt;
/// with landmark attributes and content provided by <see cref="Aside"/> components.
/// </summary>
public sealed class AsideOutlet : ComponentBase
{
    [Inject]
    private IFormFactor FormFactor { get; set; } = default!;

    [Parameter]
    public string AriaLabel { get; set; } = "Navigation Rail";

    [Parameter]
    public string NavAriaLabel { get; set; } = "Main Navigation";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "aside");
        builder.AddAttribute(1, "aria-label", AriaLabel);
        if (FormFactor.IsMobile())
        {
            builder.AddAttribute(2, "data-mobile", "true");
        }

        builder.OpenElement(3, "nav");
        builder.AddAttribute(4, "aria-label", NavAriaLabel);

        builder.OpenComponent<SectionOutlet>(5);
        builder.AddAttribute(6, nameof(SectionOutlet.SectionName), Aside.Name);
        builder.CloseComponent();

        builder.CloseElement(); // </nav>
        builder.CloseElement(); // </aside>
    }
}

