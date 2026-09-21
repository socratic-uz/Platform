using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SharedKernel.Abstractions;
using Domain.Entities;
using Shopping.Application.Protos;
using Ordering.Application.Protos;
using Identifying.Application.Protos;
using Google.Protobuf.WellKnownTypes;
using SharedKernel.ValueObjects;

namespace Infrastructure.Services;

public class OrganizationServiceClient : IServiceClient<Organization>
{
    private readonly IServiceClient<OrganizationDto> _protoClient;

    public OrganizationServiceClient(IServiceClient<OrganizationDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<Organization> Create(Organization dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<Organization> Read(Organization dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<OrganizationDto, Organization>(call, x => x);
    }

    public async Task<Organization> Update(Organization dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(Organization dto)
    {
        return _protoClient.Delete(dto);
    }
}

public class ProductServiceClient : IServiceClient<Product>
{
    private readonly IServiceClient<ProductDto> _protoClient;

    public ProductServiceClient(IServiceClient<ProductDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<Product> Create(Product dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<Product> Read(Product dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<ProductDto, Product>(call, x => x);
    }

    public async Task<Product> Update(Product dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(Product dto)
    {
        return _protoClient.Delete(dto);
    }
}

public class OrderServiceClient : IServiceClient<Order>
{
    private readonly IServiceClient<OrderDto> _protoClient;

    public OrderServiceClient(IServiceClient<OrderDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<Order> Pay(PayOrderCommand cmd)
    {
        var rawClient = _protoClient as Ordering.Application.Protos.OrderService.OrderServiceClient;
        if (rawClient == null) throw new InvalidOperationException("Raw client is not of type OrderServiceClient.");
        var result = await rawClient.PayAsync(cmd);
        return result;
    }

    public async Task<Order> Create(Order dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<Order> Read(Order dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<OrderDto, Order>(call, x => x);
    }

    public async Task<Order> Update(Order dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(Order dto)
    {
        return _protoClient.Delete(dto);
    }
}

public class OrderItemServiceClient : IServiceClient<OrderItem>
{
    private readonly IServiceClient<OrderItemDto> _protoClient;

    public OrderItemServiceClient(IServiceClient<OrderItemDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<OrderItem> Create(OrderItem dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<OrderItem> Read(OrderItem dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<OrderItemDto, OrderItem>(call, x => x);
    }

    public async Task<OrderItem> Update(OrderItem dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(OrderItem dto)
    {
        return _protoClient.Delete(dto);
    }
}

public class UserServiceClient : IServiceClient<User>
{
    private readonly IServiceClient<UserDto> _protoClient;

    public UserServiceClient(IServiceClient<UserDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<User> Create(User dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<User> Read(User dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<UserDto, User>(call, x => x);
    }

    public async Task<User> Update(User dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(User dto)
    {
        return _protoClient.Delete(dto);
    }
}

public class RoleServiceClient : IServiceClient<Role>
{
    private readonly IServiceClient<RoleDto> _protoClient;

    public RoleServiceClient(IServiceClient<RoleDto> protoClient)
    {
        _protoClient = protoClient;
    }

    public async Task<Role> Create(Role dto)
    {
        var result = await _protoClient.Create(dto);
        return result;
    }

    public AsyncServerStreamingCall<Role> Read(Role dto)
    {
        var call = _protoClient.Read(dto);
        return MappedStreamHelper.MapStreamCall<RoleDto, Role>(call, x => x);
    }

    public async Task<Role> Update(Role dto)
    {
        var result = await _protoClient.Update(dto);
        return result;
    }

    public Task<bool> Delete(Role dto)
    {
        return _protoClient.Delete(dto);
    }
}
