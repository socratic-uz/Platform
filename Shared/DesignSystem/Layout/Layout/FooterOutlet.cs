namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using static Assets.Constants.Classes.Common;
using static Assets.Constants.Classes.Footer;
using static Assets.Constants.Formats;

/// <summary>
/// Renders the semantic &lt;footer&gt; element with standard design system classes
/// and content provided by <see cref="Footer"/> components.
/// </summary>
public sealed class FooterOutlet : ComponentBase
{
    [Parameter]
    public string Class { get; set; } = string.Empty;

    private string ComputedClass =>
        string.IsNullOrEmpty(Class)
            ? string.Format(Three, AppFooter, FooterRoot, FooterContainer)
            : string.Format(Four, AppFooter, FooterRoot, FooterContainer, Class);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "footer");
        builder.AddAttribute(1, "class", ComputedClass);
        builder.OpenComponent<SectionOutlet>(2);
        builder.AddAttribute(3, nameof(SectionOutlet.SectionName), Footer.Name);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
