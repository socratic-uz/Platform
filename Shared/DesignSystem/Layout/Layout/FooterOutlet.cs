namespace Shared.Layout;

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;

/// <summary>
/// Renders the semantic &lt;footer&gt; element with landmark attributes
/// and content provided by <see cref="Footer"/> components.
/// </summary>
public sealed class FooterOutlet : ComponentBase, IDisposable
{
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "footer");
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
