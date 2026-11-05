namespace Shared.Layout;

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using static Assets.Constants.Classes.Common;
using static Assets.Constants.Classes.Footer;
using static Assets.Constants.Formats;

/// <summary>
/// Renders the semantic &lt;footer&gt; element with standard design system classes,
/// landmark attributes and content provided by <see cref="Footer"/> components.
/// </summary>
public sealed class FooterOutlet : ComponentBase, IDisposable
{
    [Parameter]
    public string Class { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        Footer.OnFooterChanged += HandleFooterChanged;
    }

    private void HandleFooterChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Footer.OnFooterChanged -= HandleFooterChanged;
    }

    private string ComputedClass =>
        string.IsNullOrEmpty(Class)
            ? string.Format(Three, AppFooter, FooterRoot, FooterContainer)
            : string.Format(Four, AppFooter, FooterRoot, FooterContainer, Class);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "footer");
        builder.AddAttribute(1, "class", ComputedClass);
        if (Footer.CurrentAttributes != null)
        {
            foreach (var attr in Footer.CurrentAttributes)
            {
                builder.AddAttribute(2, attr.Key, attr.Value);
            }
        }
        builder.OpenComponent<SectionOutlet>(3);
        builder.AddAttribute(4, nameof(SectionOutlet.SectionName), Footer.Name);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
