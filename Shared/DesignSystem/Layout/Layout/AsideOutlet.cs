namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;

/// <summary>
/// Renders the semantic &lt;aside&gt; rail element wrapping &lt;nav&gt;
/// with content provided by active page's <see cref="Aside"/> components.
/// </summary>
public sealed class AsideOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), Aside.Name);
        builder.CloseComponent();
    }
}
