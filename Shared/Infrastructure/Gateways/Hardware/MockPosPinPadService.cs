using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Hardware;
using SharedKernel.ValueObjects;

namespace Infrastructure.Gateways.Hardware;

/// <summary>
/// Реализация PosPinPadService с поддержкой эмуляции транзакций (Чип EMV / Бесконтактный NFC).
/// Поддерживает работу в Web/WASM и эмуляцию в POS (Terminal и Kiosk) без необходимости физического пин-пада.
/// </summary>
public class MockPosPinPadService : IPosPinPadService
{
    private CancellationTokenSource? _activeCts;

    public async ValueTask<PosPinPadResult> ProcessSaleAsync(
        decimal amount, 
        string orderId, 
        PosPinPadConfig config, 
        CancellationToken cancellationToken = default)
    {
        _activeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        try
        {
            // Эмуляция времени ожидания карты клиентом и ввода PIN
            await Task.Delay(1500, _activeCts.Token);

            var rrn = $"{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(100000, 999999)}";
            var authCode = $"{Random.Shared.Next(100000, 999999)}";
            var isHumo = Random.Shared.Next(2) == 0;
            var maskedPan = isHumo ? $"9860 **** **** {Random.Shared.Next(1000, 9999)}" : $"8600 **** **** {Random.Shared.Next(1000, 9999)}";
            var entryMode = isHumo ? PosCardEntryMode.ContactlessNfc : PosCardEntryMode.Chip;

            var slip = $"""
                        === POS ЧЕК (ПИН-ПАД) ===
                        ТЕРМИНАЛ: {config.TerminalId ?? "SOC_POS_PAD_01"}
                        МЕРЧАНТ: {config.MerchantId ?? "MERCH_UZ_01"}
                        RRN: {rrn}
                        AUTH CODE: {authCode}
                        КАРТА: {maskedPan}
                        СПОСОБ ВВОДА: {(entryMode == PosCardEntryMode.ContactlessNfc ? "NFC (Бесконтактно)" : "CHIP (Контактно)")}
                        СУММА: {amount:N0} UZS
                        СТАТУС: ОДОБРЕНО (00)
                        =========================
                        """;

            return PosPinPadResult.Succeeded(
                rrn: rrn,
                authCode: authCode,
                maskedPan: maskedPan,
                entryMode: entryMode,
                slipText: slip,
                cardHolder: "CARDHOLDER NAME");
        }
        catch (OperationCanceledException)
        {
            return PosPinPadResult.Failed("Операция оплаты отменена пользователем или кассиром.");
        }
        catch (Exception ex)
        {
            return PosPinPadResult.Failed($"Ошибка пин-пада: {ex.Message}");
        }
        finally
        {
            _activeCts?.Dispose();
            _activeCts = null;
        }
    }

    public ValueTask<bool> CancelOperationAsync(CancellationToken cancellationToken = default)
    {
        if (_activeCts != null && !_activeCts.IsCancellationRequested)
        {
            _activeCts.Cancel();
            return ValueTask.FromResult(true);
        }
        return ValueTask.FromResult(false);
    }
}
