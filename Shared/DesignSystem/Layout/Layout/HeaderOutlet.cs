namespace Shared.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Sections;

/// <summary>
/// Renders the semantic &lt;header&gt; element with landmark attributes
/// and content provided by <see cref="Header"/> components, analogous to <see cref="Microsoft.AspNetCore.Components.Web.HeadOutlet"/>.
/// </summary>
public sealed class HeaderOutlet : ComponentBase, System.IDisposable
{
    [Parameter]
    public bool Sticky { get; set; } = true;

    protected override void OnInitialized()
    {
        Header.OnHeaderChanged += HandleHeaderChanged;
    }

    private void HandleHeaderChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Header.OnHeaderChanged -= HandleHeaderChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "header");
        if (!Sticky)
        {
            builder.AddAttribute(1, "data-sticky", "false");
        }
        if (Header.CurrentAttributes != null)
        {
            var seq = 2;
            foreach (var kvp in Header.CurrentAttributes)
            {
                builder.AddAttribute(seq++, kvp.Key, kvp.Value);
            }
        }
        builder.OpenComponent<SectionOutlet>(10);
        builder.AddAttribute(11, nameof(SectionOutlet.SectionName), Header.Name);
        builder.CloseComponent();
        builder.CloseElement();
    }
}

/// <summary>
/// Provides content to the start zone of the header.
/// </summary>
public sealed class HeaderStart : ComponentBase
{
    internal const string Name = "header-start";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionContent>(0);
        builder.AddAttribute(1, nameof(SectionContent.SectionName), Name);
        builder.AddAttribute(2, nameof(SectionContent.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

/// <summary>
/// Provides content to the center zone of the header.
/// </summary>
public sealed class HeaderCenter : ComponentBase
{
    internal const string Name = "header-center";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionContent>(0);
        builder.AddAttribute(1, nameof(SectionContent.SectionName), Name);
        builder.AddAttribute(2, nameof(SectionContent.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

/// <summary>
/// Provides content to the end zone of the header.
/// </summary>
public sealed class HeaderEnd : ComponentBase
{
    internal const string Name = "header-end";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionContent>(0);
        builder.AddAttribute(1, nameof(SectionContent.SectionName), Name);
        builder.AddAttribute(2, nameof(SectionContent.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

/// <summary>
/// Renders content provided by <see cref="HeaderStart"/> components.
/// </summary>
public sealed class HeaderStartOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), HeaderStart.Name);
        builder.CloseComponent();
    }
}

/// <summary>
/// Renders content provided by <see cref="HeaderCenter"/> components.
/// </summary>
public sealed class HeaderCenterOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), HeaderCenter.Name);
        builder.CloseComponent();
    }
}

/// <summary>
/// Renders content provided by <see cref="HeaderEnd"/> components.
/// </summary>
public sealed class HeaderEndOutlet : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<SectionOutlet>(0);
        builder.AddAttribute(1, nameof(SectionOutlet.SectionName), HeaderEnd.Name);
        builder.CloseComponent();
    }
}
