using System;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Interfaces.Commerce;

/// <summary>
/// Поставщик UI-компонентов 28 коммерческих UX-режимов товаров (Inversion of Control для фичи Commerce).
/// Позволяет автономным модулям (Kiosk, Catalog, POS) рендерить выбор параметров товара без прямой зависимости от сборки Commerce.
/// </summary>
public interface IProductExperienceUiProvider
{
    /// <summary>
    /// Тип Razor-компонента рендерера коммерческих режимов (UniversalProductExperienceRenderer).
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? ExperienceRendererType { get; }
}
