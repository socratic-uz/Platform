namespace Smart.Web.Components;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

public partial class InvoiceViewer<TItem> : ComponentBase
{
    [Inject]
    public IStringLocalizer L { get; set; } = default!;

    protected const string DefaultOrgName = "Socratic Platform";
    protected const string DefaultInvoiceNo = "SOC-001";
    protected const string ReceiptHeaderClass = "receipt-header";

    [Parameter]
    public IEnumerable<TItem> Items { get; set; } = Array.Empty<TItem>();

    [Parameter]
    public Func<TItem, int> QuantitySelector { get; set; } = default!;

    [Parameter]
    public Func<TItem, string> NameSelector { get; set; } = default!;

    [Parameter]
    public Func<TItem, decimal> PriceSelector { get; set; } = default!;

    [Parameter]
    public EventCallback<TItem> OnItemClick { get; set; }

    [Parameter]
    public EventCallback<(TItem Item, int Delta)> OnQuantityChange { get; set; }

    [Parameter]
    public string ClientName { get; set; } = "Client Name";

    [Parameter]
    public string? OrganizationName { get; set; }

    [Parameter]
    public string? OrganizationAddress { get; set; }

    [Parameter]
    public string? InvoiceNumber { get; set; }

    [Parameter]
    public DateTime? InvoiceDate { get; set; }

    [Parameter]
    public string PaymentMethod { get; set; } = "Card";

    [Parameter]
    public string AccountNumber { get; set; } = "•••• 7744";

    [Parameter]
    public string RoutingNumber { get; set; } = "120000547";

    [Parameter]
    public string ContactEmail { get; set; } = "socratic.uz@gmail.com";

    [Parameter]
    public string ContactPhone { get; set; } = "+998958228850";

    [Parameter]
    public string Website { get; set; } = "socratic.uz";

    [Parameter]
    public string LogoUrl { get; set; } = "https://app.useanvil.com/img/email-logo-black.png";

    [Parameter]
    public string HeartIconUrl { get; set; } = string.Empty;

    public async Task HandleItemClick(TItem item)
    {
        if (OnItemClick.HasDelegate)
        {
            await OnItemClick.InvokeAsync(item);
        }
    }

    public async Task ChangeQuantity(TItem item, int delta)
    {
        if (OnQuantityChange.HasDelegate)
        {
            await OnQuantityChange.InvokeAsync((item, delta));
        }
    }
}
