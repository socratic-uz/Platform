using System;
using SharedKernel.Abstractions;

namespace Infrastructure.Services;

public class ServiceClientFactory : IServiceClientFactory
{
    private readonly IServiceProvider _provider;

    public ServiceClientFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IServiceClient<TDto> GetClient<TDto>()
    {
        var client = _provider.GetService(typeof(IServiceClient<TDto>)) as IServiceClient<TDto>;
        if (client is null) throw new InvalidOperationException($"IServiceClient<{typeof(TDto).Name}> не зарегистрирован в DI.");
        return client;
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "Service client types for DTOs are explicitly registered in DI container")]
    public object GetClient(Type dtoType)
    {
        var serviceType = typeof(IServiceClient<>).MakeGenericType(dtoType);
        var client = _provider.GetService(serviceType);
        if (client is null)
            throw new InvalidOperationException($"IServiceClient<{dtoType.Name}> не зарегистрирован в DI.");
        return client;
    }
}
