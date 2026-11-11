namespace Domain.Interfaces;

using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Платформенно-независимая абстракция для отложенной загрузки сборок Blazor.
/// Реализует паттерн «Null Object»:
/// - В WebAssembly загружает .wasm файлы по HTTP через рантайм.
/// - В Native (MAUI / POS / Hybrid) и на сервере (SSR) работает как мгновенный No-Op.
/// </summary>
public interface IModuleAssemblyLoader
{
    /// <summary>
    /// Загружает сборки, необходимые для отображения запрошенного относительного маршрута.
    /// </summary>
    Task<IReadOnlyList<Assembly>> LoadAssembliesForRouteAsync(string relativePath);
}

/// <summary>
/// No-Op реализация для окружений без отложенной загрузки (Native MAUI, Blazor Server SSR).
/// </summary>
public sealed class NoOpModuleAssemblyLoader : IModuleAssemblyLoader
{
    public static readonly NoOpModuleAssemblyLoader Instance = new();

    public Task<IReadOnlyList<Assembly>> LoadAssembliesForRouteAsync(string relativePath)
    {
        return Task.FromResult<IReadOnlyList<Assembly>>([]);
    }
}

/// <summary>
/// Декларативный реестр сопоставления URL-роутов и отложенных сборок.
/// </summary>
public static class FeatureRouteManifest
{
    public static readonly IReadOnlyDictionary<string, string[]> RouteToAssemblies =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["map"] = ["Map.wasm", "NetTopologySuite.wasm"],
            ["chat"] = ["Chat.wasm", "OpenAI.wasm", "Microsoft.Extensions.AI.wasm", "Microsoft.Extensions.AI.OpenAI.wasm"],
            ["detector"] = ["Vision.wasm", "Biometrics.Processor.wasm", "FFmpegProcessor.wasm"]
        };

    public static string[]? GetAssembliesForRoute(string path)
    {
        var cleanPath = path.TrimStart('/');
        foreach (var entry in RouteToAssemblies)
        {
            if (cleanPath.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
            {
                return entry.Value;
            }
        }
        return null;
    }
}

