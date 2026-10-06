using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Hardware;
using SharedKernel.ValueObjects;

namespace Infrastructure.Gateways.Hardware;

/// <summary>
/// Реализация CashAcceptorService для эмуляции потокового приема купюр в Kiosk.
/// </summary>
public class MockCashAcceptorService : ICashAcceptorService
{
    private CancellationTokenSource? _activeCts;
    private decimal _currentInserted = 0;

    public async ValueTask StartSessionAsync(
        decimal targetAmount, 
        CashAcceptorConfig config, 
        Action<CashInsertedEventArgs> onBillInserted, 
        CancellationToken cancellationToken = default)
    {
        _activeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _currentInserted = 0;

        try
        {
            while (_currentInserted < targetAmount && !_activeCts.Token.IsCancellationRequested)
            {
                await Task.Delay(1200, _activeCts.Token);
                
                var remaining = Math.Max(0, targetAmount - _currentInserted);
                int bill;
                if (remaining <= 20000) bill = 20000;
                else if (remaining <= 50000) bill = 50000;
                else bill = 100000;

                _currentInserted += bill;
                var rem = Math.Max(0, targetAmount - _currentInserted);
                onBillInserted(new CashInsertedEventArgs(bill, _currentInserted, rem));
            }
        }
        catch (OperationCanceledException) { }
    }

    public ValueTask<decimal> StopSessionAsync(CancellationToken cancellationToken = default)
    {
        _activeCts?.Cancel();
        return ValueTask.FromResult(_currentInserted);
    }

    public ValueTask CancelSessionAsync(CancellationToken cancellationToken = default)
    {
        _activeCts?.Cancel();
        _currentInserted = 0;
        return ValueTask.CompletedTask;
    }
}
