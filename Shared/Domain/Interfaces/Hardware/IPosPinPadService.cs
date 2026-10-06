using System.Threading;
using System.Threading.Tasks;
using SharedKernel.ValueObjects;

namespace Domain.Interfaces.Hardware;

/// <summary>
/// Аппаратный контракт для проведения безналичных операций через POS пин-пады (EMV Chip / Contactless NFC).
/// Используется в интерфейсах кассы (Terminal) и киоска самообслуживания (Kiosk).
/// </summary>
public interface IPosPinPadService
{
    ValueTask<PosPinPadResult> ProcessSaleAsync(
        decimal amount, 
        string orderId, 
        PosPinPadConfig config, 
        CancellationToken cancellationToken = default);

    ValueTask<bool> CancelOperationAsync(CancellationToken cancellationToken = default);
}
