using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Shopping.Application.Protos;
using Domain.Entities;
using File = Domain.Entities.File;

namespace Shopping.Application.Protos;

public partial class OrganizationDto
{
    public static implicit operator OrganizationDto(Organization entity)
    {
        if (entity == null) return null!;
        var proto = new OrganizationDto
        {
            Id = entity.Id?.ToString() ?? "",
            ParentId = entity.ParentId?.ToString() ?? "",
            Name = entity.Name ?? "",
            Description = entity.Description ?? "",
            ActiveUntil = entity.ActiveUntil.HasValue ? Timestamp.FromDateTime(entity.ActiveUntil.Value.ToUniversalTime()) : null
        };
        if (entity.Address != null)
        {
            proto.Address = entity.Address;
        }
        if (entity.Zones != null && entity.Zones.Count > 0)
        {
            proto.Zones.Clear();
            proto.Zones.AddRange(entity.Zones.Select(z => (SharedKernel.Protos.ZoneDto)z));
            proto.Zone = (SharedKernel.Protos.ZoneDto)entity.Zones[0];
        }
        else if (entity.Zone != null)
        {
            proto.Zone = entity.Zone;
            proto.Zones.Clear();
            proto.Zones.Add(entity.Zone);
        }
        if (entity.DefaultSchedule != null)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(entity.DefaultSchedule, SharedKernel.Serialization.BackendJsonContext.Default.WorkSchedule);
                proto.Description = (proto.Description ?? "") + "||SCHEDULE:" + json;
            }
            catch {}
        }
        if (entity.Subscription != null)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(entity.Subscription, SharedKernel.Serialization.BackendJsonContext.Default.OrganizationSubscription);
                proto.Description = (proto.Description ?? "") + "||SUBSCRIPTION:" + json;
            }
            catch {}
        }
        if (entity.Images != null && entity.Images.Count > 0)
        {
            proto.Images.AddRange(entity.Images.Select(f => (SharedKernel.Protos.File)f));
        }
        if (entity.Options != null)
        {
            proto.Options = entity.Options;
        }
        return proto;
    }

    public static implicit operator Organization(OrganizationDto proto)
    {
        if (proto == null) return null!;
        var desc = proto.Description;
        SharedKernel.ValueObjects.WorkSchedule? schedule = null;
        var scheduleJson = ExtractTaggedPayload(ref desc, "||SCHEDULE:");
        if (!string.IsNullOrEmpty(scheduleJson))
        {
            try
            {
                schedule = System.Text.Json.JsonSerializer.Deserialize(scheduleJson, SharedKernel.Serialization.BackendJsonContext.Default.WorkSchedule);
            }
            catch {}
        }

        SharedKernel.ValueObjects.OrganizationSubscription? subscription = null;
        var subJson = ExtractTaggedPayload(ref desc, "||SUBSCRIPTION:");
        if (!string.IsNullOrEmpty(subJson))
        {
            try
            {
                subscription = System.Text.Json.JsonSerializer.Deserialize(subJson, SharedKernel.Serialization.BackendJsonContext.Default.OrganizationSubscription);
            }
            catch {}
        }

        var entity = new Organization
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            ParentId = Guid.TryParse(proto.ParentId, out var pid) ? pid : (Guid?)null,
            Name = proto.Name,
            Description = desc,
            Address = proto.Address,
            Zone = proto.Zone,
            DefaultSchedule = schedule,
            Subscription = subscription ?? SharedKernel.ValueObjects.OrganizationSubscription.CreateDefaultTrial(),
            ActiveUntil = proto.ActiveUntil?.ToDateTime(),
            Images = proto.Images.Select(f => (File)f).ToList()
        };
        if (proto.Zones != null && proto.Zones.Count > 0)
        {
            entity.Zones = proto.Zones.Select(z => (SharedKernel.ValueObjects.Zone)z).ToList();
            entity.Zone = proto.Zones[0];
        }
        else if (proto.Zone != null)
        {
            entity.Zone = proto.Zone;
            entity.Zones = new List<SharedKernel.ValueObjects.Zone> { proto.Zone };
        }
        return entity;
    }

    private static string? ExtractTaggedPayload(ref string text, string tag)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var idx = text.IndexOf(tag, StringComparison.Ordinal);
        if (idx < 0) return null;

        var start = idx + tag.Length;
        var nextTagIdx = text.IndexOf("||", start, StringComparison.Ordinal);
        string payload;
        if (nextTagIdx >= 0)
        {
            payload = text.Substring(start, nextTagIdx - start);
            text = text.Substring(0, idx) + text.Substring(nextTagIdx);
        }
        else
        {
            payload = text.Substring(start);
            text = text.Substring(0, idx);
        }
        return payload;
    }
}
