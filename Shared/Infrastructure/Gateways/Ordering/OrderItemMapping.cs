using System;
using Google.Protobuf.WellKnownTypes;
using Ordering.Application.Protos;
using Domain.Entities;

namespace Ordering.Application.Protos;

public partial class OrderItemDto
{
    public static implicit operator OrderItemDto(OrderItem entity)
    {
        if (entity == null) return null!;
        return new OrderItemDto
        {
            Id = entity.Id?.ToString() ?? "",
            OrderId = entity.OrderId?.ToString() ?? "",
            ProductId = entity.ProductId?.ToString() ?? "",
            Quantity = entity.Quantity,
            Name = entity.Name ?? "",
            MakerId = entity.MakerId?.ToString() ?? "",
            Status = (int)entity.Status,
            ChangedAt = entity.ChangedAt != DateTime.MinValue ? Timestamp.FromDateTime(entity.ChangedAt.ToUniversalTime()) : null,
            Watch = entity.Watch
        };
    }

    public static implicit operator OrderItem(OrderItemDto proto)
    {
        if (proto == null) return null!;
        return new OrderItem
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            OrderId = Guid.TryParse(proto.OrderId, out var orderId) ? orderId : (Guid?)null,
            ProductId = Guid.TryParse(proto.ProductId, out var productId) ? productId : (Guid?)null,
            Quantity = (int)proto.Quantity,
            Name = proto.Name,
            MakerId = Guid.TryParse(proto.MakerId, out var makerId) ? makerId : (Guid?)null,
            Status = (SharedKernel.ValueObjects.OrderStatus)proto.Status,
            ChangedAt = proto.ChangedAt?.ToDateTime() ?? DateTime.MinValue,
            Watch = proto.Watch
        };
    }
}
