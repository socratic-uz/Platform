namespace Smart.Web.Components;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Domain.Interfaces.Common;

public partial class SmartQrCode : ComponentBase
{
    [Inject]
    private IQrDesignerService? QrDesignerService { get; set; }

    [Parameter]
    public string Data { get; set; } = string.Empty;

    [Parameter]
    public string? LogoUrl { get; set; }

    [Parameter]
    public QrStylingOptions Options { get; set; } = new();

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    public string? _svg;
    private bool _ready;
    private string? _prevData;
    private string? _prevLogo;
    private QrStylingOptions? _prevOptions;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ready = true;
            await Generate();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_ready)
            await Generate();
    }

    private async Task Generate()
    {
        var equals = Data == _prevData &&
                     LogoUrl == _prevLogo &&
                     _prevOptions != null &&
                     Options.PrimaryColor == _prevOptions.PrimaryColor &&
                     Options.BackgroundColor == _prevOptions.BackgroundColor &&
                     Options.CornerColor == _prevOptions.CornerColor &&
                     Options.CornerDotColor == _prevOptions.CornerDotColor &&
                     Options.DotType == _prevOptions.DotType &&
                     Options.CornerType == _prevOptions.CornerType &&
                     Options.CornerDotType == _prevOptions.CornerDotType &&
                     Options.ImageSizeFactor == _prevOptions.ImageSizeFactor &&
                     Options.Margin == _prevOptions.Margin &&
                     Options.UseBackgroundGradient == _prevOptions.UseBackgroundGradient &&
                     Options.BackgroundGradientType == _prevOptions.BackgroundGradientType &&
                     Options.BackgroundGradientRotation == _prevOptions.BackgroundGradientRotation &&
                     Options.BackgroundGradientStartColor == _prevOptions.BackgroundGradientStartColor &&
                     Options.BackgroundGradientEndColor == _prevOptions.BackgroundGradientEndColor;

        if (equals) return;

        _prevData = Data;
        _prevLogo = LogoUrl;
        _prevOptions = Options;

        if (string.IsNullOrEmpty(Data) || QrDesignerService == null)
        {
            _svg = null;
        }
        else
        {
            try
            {
                _svg = await QrDesignerService.GenerateQrCodeSvgAsync(Data, LogoUrl ?? string.Empty, Options);
            }
            catch
            {
                _svg = null;
            }
        }
        StateHasChanged();
    }
}
