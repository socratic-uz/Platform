using System;
using SharedKernel.ValueObjects;
using Identifying.Application.Protos;
using Domain.Entities;

namespace Identifying.Application.Protos;

public partial class RoleDto
{
    public static implicit operator RoleDto(Role entity)
    {
        if (entity == null) return null!;
        return new RoleDto
        {
            Id = entity.Id?.ToString() ?? "",
            UserId = entity.UserId?.ToString() ?? "",
            OrganizationId = entity.OrganizationId?.ToString() ?? "",
            Name = entity.Name ?? "",
            Permission = (long)entity.Permission
        };
    }

    public static implicit operator Role(RoleDto proto)
    {
        if (proto == null) return null!;
        return new Role
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            UserId = Guid.TryParse(proto.UserId, out var userId) ? userId : (Guid?)null,
            OrganizationId = Guid.TryParse(proto.OrganizationId, out var orgId) ? orgId : (Guid?)null,
            Name = proto.Name,
            Permission = (Permission)proto.Permission
        };
    }
}
