using System;
using SharedKernel.ValueObjects;
using Identifying.Application.Protos;
using Domain.Entities;
using File = Domain.Entities.File;

namespace Identifying.Application.Protos;

public partial class ProfileDto
{
    public static implicit operator ProfileDto(User entity)
    {
        if (entity == null) return null!;
        var proto = new ProfileDto
        {
            Id = entity.Id?.ToString() ?? "",
            Phone = entity.Phone ?? "",
            Language = (int)entity.Language,
            Theme = (int)entity.Theme,
            Accent = entity.Accent
        };
        if (entity.Name != null)
        {
            proto.Name = entity.Name;
        }
        if (entity.Image != null)
        {
            proto.Image = (SharedKernel.Protos.File)entity.Image;
        }
        if (entity.Address != null)
        {
            proto.Address = entity.Address;
        }
        return proto;
    }

    public static implicit operator User(ProfileDto proto)
    {
        if (proto == null) return null!;
        return new User
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            Phone = proto.Phone,
            Name = proto.Name,
            Image = proto.Image != null ? (File)proto.Image : null,
            Address = proto.Address,
            Language = (Language)proto.Language,
            Theme = (Theme)proto.Theme,
            Accent = proto.Accent
        };
    }
}
