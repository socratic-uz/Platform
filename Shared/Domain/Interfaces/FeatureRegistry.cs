using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Interfaces;

/// <summary>
/// Централизованный реестр подключенных фиче-модулей фронтенда.
/// Управляет регистрацией сервисов и передачей сборок в Router (AdditionalAssemblies).
/// </summary>
public static class FeatureRegistry
{
    private static readonly ConcurrentDictionary<string, IFeatureModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Регистрирует фиче-модуль в реестре экосистемы.
    /// </summary>
    public static void Register(IFeatureModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        _modules[module.Name] = module;
    }

    /// <summary>
    /// Статическая регистрация фиче-модуля по типу (AOT-safe, zero reflection).
    /// </summary>
    public static void Register<T>() where T : IFeatureModule, new()
    {
        Register(new T());
    }

    /// <summary>
    /// Пакетная статическая регистрация фиче-модулей (AOT-safe, zero reflection).
    /// </summary>
    public static void Register(params IFeatureModule[] modules)
    {
        if (modules == null) return;
        foreach (var module in modules)
        {
            if (module != null)
            {
                Register(module);
            }
        }
    }

    /// <summary>
    /// Возвращает список всех зарегистрированных фиче-модулей, отсортированных по Order.
    /// </summary>
    public static IReadOnlyCollection<IFeatureModule> GetAllModules()
    {
        return _modules.Values.OrderBy(m => m.Order).ToArray();
    }

    /// <summary>
    /// Возвращает массив уникальных сборок всех модулей для передачи в Blazor Router.AdditionalAssemblies.
    /// </summary>
    public static Assembly[] GetFeatureAssemblies()
    {
        return _modules.Values
            .Select(m => m.Assembly)
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Выполняет инициализацию сервисов всех зарегистрированных модулей в DI контейнере.
    /// </summary>
    public static IServiceCollection AddRegisteredFeatureServices(this IServiceCollection services, IConfiguration configuration)
    {
        foreach (var module in GetAllModules())
        {
            module.RegisterServices(services, configuration);
        }
        return services;
    }
}
