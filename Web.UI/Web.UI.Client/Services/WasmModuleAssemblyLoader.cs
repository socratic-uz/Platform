namespace Web.UI.Client.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Domain.Interfaces;

/// <summary>
/// Реализация IModuleAssemblyLoader для WebAssembly.
/// Загружает отложенные сборки (.wasm) через LazyAssemblyLoader браузерного рантайма
/// и регистрирует найденные IFeatureModule в FeatureRegistry.
/// </summary>
public sealed class WasmModuleAssemblyLoader : IModuleAssemblyLoader
{
    private readonly LazyAssemblyLoader _lazyLoader;
    private readonly HashSet<string> _loadedAssemblyNames = new(StringComparer.OrdinalIgnoreCase);

    public WasmModuleAssemblyLoader(LazyAssemblyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }

    public async Task<IReadOnlyList<Assembly>> LoadAssembliesForRouteAsync(string relativePath)
    {
        var targetAssemblies = FeatureRouteManifest.GetAssembliesForRoute(relativePath);
        if (targetAssemblies == null || targetAssemblies.Length == 0)
        {
            return [];
        }

        var assembliesToFetch = targetAssemblies
            .Where(name => !_loadedAssemblyNames.Contains(name))
            .ToArray();

        if (assembliesToFetch.Length == 0)
        {
            return [];
        }

        var loaded = await _lazyLoader.LoadAssembliesAsync(assembliesToFetch);
        var result = new List<Assembly>();

        foreach (var asm in loaded)
        {
            var asmName = asm.GetName().Name ?? string.Empty;
            _loadedAssemblyNames.Add(asmName);
            _loadedAssemblyNames.Add(asmName + ".wasm");
            result.Add(asm);

            RegisterAssemblyModules(asm);
        }

        return result;
    }

    private static void RegisterAssemblyModules(Assembly assembly)
    {
        try
        {
            var moduleTypes = assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IFeatureModule).IsAssignableFrom(t));

            foreach (var type in moduleTypes)
            {
                if (Activator.CreateInstance(type) is IFeatureModule module)
                {
                    FeatureRegistry.Register(module);
                }
            }
        }
        catch
        {
            // Игнорируем ошибки рефлексии при тримминге
        }
    }
}

