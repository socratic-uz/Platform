namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;

/// <summary>
/// Renders the &lt;header&gt; element provided by the active page's <see cref="Header"/> component.
/// </summary>
public sealed class HeaderOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), Header.Name);
        builder.CloseComponent();
    }
}
