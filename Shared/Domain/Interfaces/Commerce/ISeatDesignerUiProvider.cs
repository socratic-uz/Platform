using System;

namespace Domain.Interfaces.Commerce;

/// <summary>
/// Порт для динамического предоставления компонентов интерактивной схемы рассадки (SeatDesigner).
/// Устраняет прямую зависимость модуля Commerce от реализации SeatDesigner.
/// </summary>
public interface ISeatDesignerUiProvider
{
    /// <summary>
    /// Возвращает тип Razor-компонента интерактивного холста рассадки.
    /// </summary>
    Type? GetCanvasComponentType();

    /// <summary>
    /// Возвращает тип Razor-компонента панели инструментов редактора схемы.
    /// </summary>
    Type? GetToolbarComponentType();
}
