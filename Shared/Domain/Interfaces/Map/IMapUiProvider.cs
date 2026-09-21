using System;

namespace Domain.Interfaces.Map;

/// <summary>
/// Порт для динамического предоставления компонентов картографии и выбора геолокации.
/// Устраняет жесткую связанность между фичами (Commerce, Delivery, POS) и модулем Map.
/// </summary>
public interface IMapUiProvider
{
    /// <summary>
    /// Возвращает тип Razor-компонента интерактивного выбора точки на карте.
    /// </summary>
    Type? GetPointPickerComponentType();

    /// <summary>
    /// Возвращает тип Razor-компонента выбора/проверки геозон доставки.
    /// </summary>
    Type? GetZonePickerComponentType();
}
