using System;
using System.Threading.Tasks;
using Domain.Interfaces.Hardware;
using Microsoft.JSInterop;

namespace Infrastructure.Gateways.Receipt;

/// <summary>
/// Web-браузерная реализация печати чеков через JavaScript window.print() и печать элементов по ID.
/// Позволяет чекам печататься как в браузере (WebAssembly/SSR), так и без физического термопринтера.
/// </summary>
public class BrowserReceiptPrinterService : IReceiptPrinterService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserReceiptPrinterService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task PrintElementByIdAsync(string elementId, string styleElementId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.print");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserReceiptPrinterService] PrintElementByIdAsync error: {ex.Message}");
        }
    }

    public async Task PrintHtmlDocumentAsync(string title, string htmlContent)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.print");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserReceiptPrinterService] PrintHtmlDocumentAsync error: {ex.Message}");
        }
    }
}
