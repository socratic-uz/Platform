namespace Domain.State;

using System;
using System.Collections.Generic;
using SharedKernel.ValueObjects;

/// <summary>
/// Реактивное хранилище клиентской корзины (Client-Side State Container).
/// Инкапсулирует состояние позиций, скидок и инварианты корзины.
/// 100% Native AOT, потокобезопасно.
/// </summary>
public interface ICartStore
{
    TransientCart CurrentCart { get; }
    IReadOnlyList<CartItem> Items { get; }
    decimal SubTotal { get; }
    decimal DiscountAmount { get; }
    decimal ShippingFee { get; }
    decimal GrandTotal { get; }
    string? AppliedPromoCode { get; }

    event Action? OnChanged;

    void AddItem(CartItem item);
    void RemoveItem(string productId);
    void UpdateQuantity(string productId, decimal delta);
    void ApplyPromoCode(string promoCode, decimal discountAmount);
    void ClearPromoCode();
    void SetShippingFee(decimal fee);
    void Clear();
}
