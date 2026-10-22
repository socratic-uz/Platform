using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SharedKernel.Fiscalization;
using SharedKernel.ValueObjects;
using Domain.Interfaces.Common;
using Domain.Interfaces.Hardware;

namespace Smart.Web.Components;

public partial class UniversalReceiptView : ComponentBase
{
    [Inject] public IReceiptPrinterService? PrinterService { get; set; }
    [Inject] public IStringLocalizer L { get; set; } = default!;
    [Inject] public IServiceProvider ServiceProvider { get; set; } = default!;

    public const string ReceiptTerminalLabel = global::Assets.Constants.Loc.Receipt.TerminalLabel;
    public const string ReceiptPaymentMethod = global::Assets.Constants.Loc.Receipt.PaymentMethod;
    public const string CurrencyUzs = global::Assets.Constants.Loc.Common.CurrencyUzs;

    [Parameter] public string OrganizationName { get; set; } = "Socratic Ecosystem Partner";
    [Parameter] public string TaxId { get; set; } = "309182736";
    [Parameter] public string TerminalId { get; set; } = "SOC_TERM_01";
    [Parameter] public long ReceiptSeq { get; set; } = 1042;
    [Parameter] public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    [Parameter] public List<FiscalItem> Items { get; set; } = new();
    [Parameter] public decimal TotalAmount { get; set; } = 120000;
    [Parameter] public decimal TotalVatAmount { get; set; } = 12857;
    [Parameter] public string PaymentGatewayName { get; set; } = "Click";
    [Parameter] public string PaymentTypeName { get; set; } = "Онлайн-карта";
    [Parameter] public string FiscalSign { get; set; } = "8F1A29D4E03B";
    [Parameter] public string SoliqQrUrl { get; set; } = "https://ofd.soliq.uz/check?t=SOC_TERM_01&r=1042&c=20260823103000&s=8F1A29D4E03B";

    protected string? QrSvg { get; set; }

    private readonly QrStylingOptions qrOptions = new()
    {
        Size = 110,
        Margin = 2,
        PrimaryColor = "#0f172a",
        BackgroundColor = "#ffffff",
        CornerColor = "#0284c7",
        CornerDotColor = "#0284c7",
        DotType = "square",
        CornerType = "square",
        CornerDotType = "dot"
    };

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SoliqQrUrl))
        {
            var qrService = ServiceProvider.GetService(typeof(IQrDesignerService)) as IQrDesignerService;
            if (qrService != null)
            {
                try
                {
                    QrSvg = await qrService.GenerateQrCodeSvgAsync(SoliqQrUrl, "", qrOptions);
                }
                catch
                {
                    QrSvg = null;
                }
            }
        }
    }

    protected async Task PrintReceiptAsync()
    {
        if (PrinterService != null)
        {
            await PrinterService.PrintElementByIdAsync("fiscal-receipt-element", "receipt-styles");
        }
        else
        {
            var js = ServiceProvider.GetService(typeof(IJSRuntime)) as IJSRuntime;
            if (js != null)
            {
                try
                {
                    await js.InvokeVoidAsync("window.print");
                }
                catch { }
            }
        }
    }
}
