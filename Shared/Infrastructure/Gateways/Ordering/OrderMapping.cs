using System;
using Google.Protobuf.WellKnownTypes;
using SharedKernel.ValueObjects;
using Ordering.Application.Protos;
using Domain.Entities;

namespace Ordering.Application.Protos;

public partial class OrderDto
{
    public static implicit operator OrderDto(Order entity)
    {
        if (entity == null) return null!;
        return new OrderDto
        {
            Id = entity.Id?.ToString() ?? "",
            OrganizationId = entity.OrganizationId?.ToString() ?? "",
            RoleId = entity.RoleId?.ToString() ?? "",
            PlaceId = entity.PlaceId?.ToString() ?? "",
            Status = (int)entity.Status,
            PaymentType = (int)entity.PaymentType,
            TotalAmount = (double)entity.TotalAmount,
            RequiresOtp = entity.RequiresOtp,
            OtpToken = entity.OtpToken ?? "",
            PaymentMessage = entity.PaymentMessage ?? "",
            CreatedAt = entity.CreatedAt != DateTime.MinValue ? Timestamp.FromDateTime(entity.CreatedAt.ToUniversalTime()) : null,
            Watch = entity.Watch
        };
    }

    public static implicit operator Order(OrderDto proto)
    {
        if (proto == null) return null!;
        return new Order
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            OrganizationId = Guid.TryParse(proto.OrganizationId, out var orgId) ? orgId : (Guid?)null,
            RoleId = Guid.TryParse(proto.RoleId, out var roleId) ? roleId : (Guid?)null,
            PlaceId = Guid.TryParse(proto.PlaceId, out var placeId) ? placeId : (Guid?)null,
            Status = (OrderStatus)proto.Status,
            PaymentType = (PaymentMethod)proto.PaymentType,
            TotalAmount = (decimal)proto.TotalAmount,
            RequiresOtp = proto.RequiresOtp,
            OtpToken = proto.OtpToken,
            PaymentMessage = proto.PaymentMessage,
            CreatedAt = proto.CreatedAt?.ToDateTime() ?? DateTime.MinValue,
            Watch = proto.Watch
        };
    }
}
