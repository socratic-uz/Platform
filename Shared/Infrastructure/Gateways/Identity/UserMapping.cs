using System;
using Google.Protobuf.WellKnownTypes;
using Identifying.Application.Protos;
using Domain.Entities;
using File = Domain.Entities.File;

namespace Identifying.Application.Protos;

public partial class UserDto
{
    public static implicit operator UserDto(User entity)
    {
        if (entity == null) return null!;
        return new UserDto
        {
            Id = entity.Id?.ToString() ?? "",
            Expires = entity.Expires.HasValue ? Timestamp.FromDateTime(entity.Expires.Value.ToUniversalTime()) : null,
            Phone = entity.Phone ?? "",
            Email = entity.Email ?? "",
            Name = entity.Name,
            Address = entity.Address,
            Image = entity.Image != null ? new SharedKernel.Protos.File
            {
                Url = entity.Image.Url ?? "",
                FileName = entity.Image.FileName ?? "",
                ContentType = entity.Image.ContentType ?? "",
                Content = entity.Image.Content != null ? Google.Protobuf.ByteString.CopyFrom(entity.Image.Content) : Google.Protobuf.ByteString.Empty
            } : null,
            IsBlocked = entity.IsBlocked,
            Language = (int)entity.Language,
            Theme = (int)entity.Theme,
            Accent = entity.Accent
        };
    }

    public static implicit operator User(UserDto proto)
    {
        if (proto == null) return null!;
        return new User
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            Expires = proto.Expires?.ToDateTime(),
            Phone = proto.Phone,
            Email = proto.Email,
            Name = proto.Name,
            Address = proto.Address,
            Image = proto.Image != null ? new File
            {
                Url = proto.Image.Url,
                FileName = proto.Image.FileName,
                ContentType = proto.Image.ContentType,
                Content = proto.Image.Content?.ToByteArray()
            } : null,
            IsBlocked = proto.IsBlocked,
            Language = (SharedKernel.ValueObjects.Language)proto.Language,
            Theme = (SharedKernel.ValueObjects.Theme)proto.Theme,
            Accent = proto.Accent
        };
    }
}
