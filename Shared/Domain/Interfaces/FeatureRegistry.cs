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
    /// Автоматически обнаруживает и регистрирует все фиче-модули (IFeatureModule)
    /// из всех доступных и загруженных в процесс сборок.
    /// Позволяет Web.UI работать как универсальный Plug &amp; Play хост в любом окружении.
    /// </summary>
    public static void DiscoverAndRegisterModules()
    {
        var loadedAssemblies = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies());

        foreach (var assembly in loadedAssemblies.ToArray())
        {
            try
            {
                if (assembly.IsDynamic) continue;
                foreach (var refName in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        if (!loadedAssemblies.Any(a => string.Equals(a.GetName().Name, refName.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            var loaded = Assembly.Load(refName);
                            loadedAssemblies.Add(loaded);
                        }
                    }
                    catch
                    {
                        // Игнорируем невозможность загрузить специфичные зависимости
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки доступа к метаданным
            }
        }

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir) && Directory.Exists(baseDir))
            {
                foreach (var dllPath in Directory.GetFiles(baseDir, "*.dll"))
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(dllPath);
                        if (!loadedAssemblies.Any(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            var loaded = Assembly.Load(assemblyName);
                            loadedAssemblies.Add(loaded);
                        }
                    }
                    catch
                    {
                        // Игнорируем ненативные или несовместимые библиотеки
                    }
                }
            }
        }
        catch
        {
            // Игнорируем в средах без доступа к файловой системе (например, WASM)
        }

        foreach (var assembly in loadedAssemblies)
        {
            try
            {
                if (assembly.IsDynamic) continue;
                var moduleTypes = assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IFeatureModule).IsAssignableFrom(t));

                foreach (var type in moduleTypes)
                {
                    if (Activator.CreateInstance(type) is IFeatureModule module)
                    {
                        Register(module);
                    }
                }
            }
            catch
            {
                // Пропускаем сборки без возможности инспекции типов
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
