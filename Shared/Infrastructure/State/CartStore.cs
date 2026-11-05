namespace Infrastructure.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Domain.State;
using SharedKernel.ValueObjects;

/// <summary>
/// Потокобезопасная реализация хранилища корзины в памяти (Client-Side In-Memory Cart Store).
/// 100% Native AOT, без рефлексии.
/// </summary>
public sealed class CartStore : ICartStore
{
    private readonly object _sync = new();
    private readonly TransientCart _cart = new();

    public TransientCart CurrentCart => _cart;

    public IReadOnlyList<CartItem> Items
    {
        get
        {
            lock (_sync)
            {
                return _cart.Items.ToList();
            }
        }
    }

    public decimal SubTotal
    {
        get
        {
            lock (_sync)
            {
                return _cart.TotalAmount;
            }
        }
    }

    public decimal DiscountAmount { get; private set; }
    public decimal ShippingFee { get; private set; }

    public decimal GrandTotal
    {
        get
        {
            lock (_sync)
            {
                return Math.Max(0, _cart.TotalAmount - DiscountAmount + ShippingFee);
            }
        }
    }

    public string? AppliedPromoCode { get; private set; }

    public event Action? OnChanged;

    public void AddItem(CartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync)
        {
            var existing = _cart.Items.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                i.SelectedVariantId == item.SelectedVariantId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                _cart.Items.Add(item);
            }
            _cart.UpdatedAt = DateTime.UtcNow;
        }
        NotifyStateChanged();
    }

    public void RemoveItem(string productId)
    {
        lock (_sync)
        {
            _cart.Items.RemoveAll(i => i.ProductId == productId);
            _cart.UpdatedAt = DateTime.UtcNow;
        }
        NotifyStateChanged();
    }

    public void UpdateQuantity(string productId, decimal delta)
    {
        lock (_sync)
        {
            var existing = _cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += delta;
                if (existing.Quantity <= 0)
                {
                    _cart.Items.Remove(existing);
                }
                _cart.UpdatedAt = DateTime.UtcNow;
            }
        }
        NotifyStateChanged();
    }

    public void ApplyPromoCode(string promoCode, decimal discountAmount)
    {
        lock (_sync)
        {
            AppliedPromoCode = promoCode;
            DiscountAmount = Math.Max(0, discountAmount);
        }
        NotifyStateChanged();
    }

    public void ClearPromoCode()
    {
        lock (_sync)
        {
            AppliedPromoCode = null;
            DiscountAmount = 0;
        }
        NotifyStateChanged();
    }

    public void SetShippingFee(decimal fee)
    {
        lock (_sync)
        {
            ShippingFee = Math.Max(0, fee);
        }
        NotifyStateChanged();
    }

    public void Clear()
    {
        lock (_sync)
        {
            _cart.Items.Clear();
            AppliedPromoCode = null;
            DiscountAmount = 0;
            ShippingFee = 0;
            _cart.UpdatedAt = DateTime.UtcNow;
        }
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChanged?.Invoke();
}
