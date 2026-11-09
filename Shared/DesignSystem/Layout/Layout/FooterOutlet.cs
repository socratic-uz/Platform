namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;

/// <summary>
/// Renders the semantic &lt;footer&gt; element provided by active page's <see cref="Footer"/> components.
/// </summary>
public sealed class FooterOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), Footer.Name);
        builder.CloseComponent();
    }
}
