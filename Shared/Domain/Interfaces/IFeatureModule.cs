using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Interfaces;

/// <summary>
/// Представляет контракт автономного функционального модуля (Feature Slice / Micro-App) в экосистеме Socratic.
/// Позволяет подключать фичи по принципу «Plug and Play» без модификации маршрутов или хост-приложений.
/// </summary>
public interface IFeatureModule
{
    /// <summary>
    /// Уникальный системный идентификатор фичи (e.g., "Chat", "Map", "SeatDesigner").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Отображаемое наименование фичи в навигации / меню.
    /// </summary>
    string DisplayTitle { get; }

    /// <summary>
    /// Префикс URL-маршрутов для фичи (e.g., "/chat", "/map", "/seat-designer").
    /// </summary>
    string RoutePrefix { get; }

    /// <summary>
    /// Иконка Material Design 3 для отображения в панели навигации (e.g., "chat", "map", "event_seat").
    /// </summary>
    string? Icon { get; }

    /// <summary>
    /// Порядковый номер сортировки в меню.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Сборка с Razor-страницами и компонентами для автоматической регистрации роутером Blazor.
    /// </summary>
    Assembly Assembly { get; }

    /// <summary>
    /// Регистрация специфичных для фичи сервисов, стейт-сторов и клиентов в DI-контейнере.
    /// </summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
