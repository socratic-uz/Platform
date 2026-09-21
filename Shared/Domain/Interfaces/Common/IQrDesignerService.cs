using System.Threading.Tasks;

namespace Domain.Interfaces.Common;

/// <summary>
/// Контракт сервиса генерации стилизованных QR-кодов.
/// </summary>
public interface IQrDesignerService
{
    Task<string> GenerateQrCodeSvgAsync(string plainText, string logoUrl, QrStylingOptions? options = null);
}
