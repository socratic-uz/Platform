using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Authorization;
using SharedKernel.Abstractions;

namespace Shared.Services;

/// <summary>
/// Автономный провайдер аутентификации для работы изолированных Host-стендов (F5 песочниц)
/// без поднятого микросервиса Identifying.
/// </summary>
public class StandaloneAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState DefaultState = new(
        new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new Claim(ClaimTypes.Name, "Dev Cashier"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("OrganizationId", "00000000-0000-0000-0000-000000000001")
        ], "StandaloneAuth")));

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(DefaultState);
}

/// <summary>
/// Автономная фабрика клиентских сервисов gRPC для локальной разработки и стендов без бэкенда.
/// Если реальный gRPC сервис не зарегистрирован, возвращает безопасный Null-клиент.
/// </summary>
public class StandaloneServiceClientFactory : IServiceClientFactory
{
    private readonly IServiceProvider _provider;

    public StandaloneServiceClientFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IServiceClient<TDto> GetClient<TDto>()
    {
        var client = _provider.GetService(typeof(IServiceClient<TDto>)) as IServiceClient<TDto>;
        return client ?? new NullServiceClient<TDto>();
    }

    public object GetClient(Type dtoType)
    {
        var serviceType = typeof(IServiceClient<>).MakeGenericType(dtoType);
        var client = _provider.GetService(serviceType);
        if (client is not null) return client;

        var nullClientType = typeof(NullServiceClient<>).MakeGenericType(dtoType);
        return Activator.CreateInstance(nullClientType)!;
    }

    private sealed class NullServiceClient<T> : IServiceClient<T>
    {
        public Task<T> Create(T dto) => Task.FromResult(dto);
        public AsyncServerStreamingCall<T> Read(T dto) => null!;
        public Task<T> Update(T dto) => Task.FromResult(dto);
        public Task<bool> Delete(T dto) => Task.FromResult(true);
    }
}
