using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Markdig;

namespace Markdown.Web;

public partial class MarkdownSection : ComponentBase
{
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IJSObjectReference _jsModule = default!;
    private string ComputeClass() => string.IsNullOrWhiteSpace(Class) ? "markdown-body" : $"markdown-body {Class}";
    private bool _markdownChanged = false;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IStaticAssetService StaticAssetService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Markdown content
    /// </summary>
    [Parameter] public string? Content { get; set; }

    /// <summary>
    /// Gets or sets asset to read the Markdown from
    /// </summary>
    [Parameter] public string? FromAsset { get; set; }

    private string? _previousContent;
    private string? _previousFromAsset;

    protected override void OnParametersSet()
    {
        if (Content != _previousContent || FromAsset != _previousFromAsset)
        {
            _previousContent = Content;
            _previousFromAsset = FromAsset;
            _markdownChanged = true;
        }
    }

    [Parameter]
    public EventCallback OnContentConverted { get; set; }

    public MarkupString HtmlContent { get; private set; }

    protected override void OnInitialized()
    {
        if (Content is null && string.IsNullOrEmpty(FromAsset))
        {
            throw new ArgumentException("You need to provide either Content or FromAsset parameter");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // import code for highlighting code blocks
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Markdown/Components/MarkdownSection.razor.js");
            await _jsModule.InvokeVoidAsync("runMermaid");
        }

        if (firstRender || _markdownChanged)
        {
            _markdownChanged = false;

            // create markup from markdown source
            HtmlContent = await MarkdownToMarkupStringAsync();



            StateHasChanged();

            // notify that content converted from markdown
            if (OnContentConverted.HasDelegate)
            {
                await OnContentConverted.InvokeAsync();
            }
            await _jsModule.InvokeVoidAsync("highlight");
            await _jsModule.InvokeVoidAsync("addCopyButton");

            await _jsModule.InvokeVoidAsync("runMermaid");

        }
    }

    /// <summary>
    /// Converts markdown, provided in Content or from markdown file stored as a static asset, to MarkupString for rendering.
    /// </summary>
    /// <returns>MarkupString</returns>
    private async Task<MarkupString> MarkdownToMarkupStringAsync()
    {
        string? markdown;
        if (string.IsNullOrEmpty(FromAsset))
        {
            markdown = Content;
        }
        else
        {
            markdown = await StaticAssetService.GetAsync(FromAsset);
        }

        return ConvertToMarkupString(markdown);
    }
    private static MarkupString ConvertToMarkupString(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var builder = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use<MarkdownSectionPreCodeExtension>();

            var pipeline = builder.Build();

            // Convert markdown string to HTML
            var html = Markdig.Markdown.ToHtml(value, pipeline);

            // Return sanitized HTML as a MarkupString that Blazor can render
            return new MarkupString(html);
        }

        return new MarkupString();
    }
}

