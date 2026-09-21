using System.Threading.Tasks;

namespace Domain.Interfaces.Hardware;

/// <summary>
/// Аппаратный контракт для печати чеков и документов на кассовых и термопринтерах (ESC/POS / Browser Print).
/// </summary>
public interface IReceiptPrinterService
{
    Task PrintElementByIdAsync(string elementId, string styleElementId);
    Task PrintHtmlDocumentAsync(string title, string htmlContent);
}
