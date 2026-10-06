using System;
using System.Threading;
using System.Threading.Tasks;
using SharedKernel.ValueObjects;

namespace Domain.Interfaces.Hardware;

public readonly record struct CashInsertedEventArgs(int Denomination, decimal TotalInserted, decimal Remaining);

/// <summary>
/// Аппаратный контракт для потокового приёма наличных купюр через купюроприёмники (CCNET / ID-003 / SSP).
/// </summary>
public interface ICashAcceptorService
{
    ValueTask StartSessionAsync(
        decimal targetAmount, 
        CashAcceptorConfig config, 
        Action<CashInsertedEventArgs> onBillInserted, 
        CancellationToken cancellationToken = default);

    ValueTask<decimal> StopSessionAsync(CancellationToken cancellationToken = default);

    ValueTask CancelSessionAsync(CancellationToken cancellationToken = default);
}
