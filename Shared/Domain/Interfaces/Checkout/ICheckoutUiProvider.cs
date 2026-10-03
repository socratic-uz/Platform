using System;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Interfaces.Checkout;

/// <summary>
/// Поставщик UI-компонентов оформления заказа (Inversion of Control для фичи Checkout).
/// Позволяет автономным модулям (Kiosk, POS, Catalog) рендерить оформление заказа без прямой зависимости от сборки Checkout.
/// </summary>
public interface ICheckoutUiProvider
{
    /// <summary>
    /// Тип Razor-компонента универсальной панели оформления заказа (UniversalCheckoutPanel).
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? CheckoutPanelType { get; }

    /// <summary>
    /// Тип Razor-компонента боковой панели корзины/заказа (OrderDrawer).
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? OrderDrawerType { get; }
}
