namespace Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Domain.Interfaces;

public partial class Routes : ComponentBase
{
    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public IServiceProvider ServiceProvider { get; set; } = default!;

    private IModuleAssemblyLoader AssemblyLoader =>
        ServiceProvider.GetService(typeof(IModuleAssemblyLoader)) as IModuleAssemblyLoader
        ?? NoOpModuleAssemblyLoader.Instance;

    private readonly List<Assembly> _lazyLoadedAssemblies = new();

    private IEnumerable<Assembly> GetAdditionalAssemblies()
    {
        return FeatureRegistry.GetFeatureAssemblies()
            .Concat(_lazyLoadedAssemblies)
            .Distinct();
    }

    private async Task OnNavigateAsync(NavigationContext context)
    {
        try
        {
            var loaded = await AssemblyLoader.LoadAssembliesForRouteAsync(context.Path);
            if (loaded.Count > 0)
            {
                foreach (var asm in loaded)
                {
                    if (!_lazyLoadedAssemblies.Contains(asm))
                    {
                        _lazyLoadedAssemblies.Add(asm);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Routes] Navigation assembly load error: {ex.Message}");
        }
    }
}
