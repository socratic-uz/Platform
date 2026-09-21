using System;
using System.Linq;
using Shopping.Application.Protos;
using Domain.Entities;
using File = Domain.Entities.File;

namespace Shopping.Application.Protos;

public partial class ProductDto
{
    public static implicit operator ProductDto(Product entity)
    {
        if (entity == null) return null!;
        var proto = new ProductDto
        {
            Id = entity.Id?.ToString() ?? "",
            OrganizationId = entity.OrganizationId?.ToString() ?? "",
            ParentId = entity.ParentId?.ToString() ?? "",
            Name = entity.Name ?? "",
            Price = (double)entity.Price,
            Description = entity.Description ?? "",
            Unit = entity.Unit ?? "",
            IsWished = entity.IsWished,
            Type = string.IsNullOrEmpty(entity.Type) ? 0 : int.TryParse(entity.Type, out var t) ? t : 0,
            Weight = entity.Weight,
            Volume = entity.Volume,
            IsAgeRestricted = entity.IsAgeRestricted,
            ProductUxModeValue = entity.ProductUxModeValue
        };
        if (entity.Images != null && entity.Images.Count > 0)
        {
            proto.Images.AddRange(entity.Images.Select(f => (SharedKernel.Protos.File)f));
        }
        if (entity.Tags != null && entity.Tags.Count > 0)
        {
            proto.Tags.AddRange(entity.Tags);
        }
        if (entity.AllowedZoneIds != null && entity.AllowedZoneIds.Count > 0)
        {
            proto.AllowedZoneIds.AddRange(entity.AllowedZoneIds);
        }
        return proto;
    }

    public static implicit operator Product(ProductDto proto)
    {
        if (proto == null) return null!;
        var uxMode = InferUxModeValue(proto);
        return new Product
        {
            Id = Guid.TryParse(proto.Id, out var id) ? id : (Guid?)null,
            OrganizationId = Guid.TryParse(proto.OrganizationId, out var orgId) ? orgId : (Guid?)null,
            ParentId = Guid.TryParse(proto.ParentId, out var pid) ? pid : (Guid?)null,
            Name = proto.Name,
            Price = (decimal)proto.Price,
            Description = proto.Description,
            Type = proto.Type.ToString(),
            Images = proto.Images.Select(f => (File)f).ToList(),
            Unit = proto.Unit,
            Tags = proto.Tags.ToList(),
            IsWished = proto.IsWished,
            Weight = proto.Weight,
            Volume = proto.Volume,
            IsAgeRestricted = proto.IsAgeRestricted,
            AllowedZoneIds = proto.AllowedZoneIds.ToList(),
            ProductUxModeValue = uxMode
        };
    }

    public static int InferUxModeValue(ProductDto proto)
    {
        if (proto.ProductUxModeValue > 0) return proto.ProductUxModeValue;

        if (proto.CustomAttributes != null && proto.CustomAttributes.TryGetValue("UxMode", out var uStr) && int.TryParse(uStr, out var uVal) && uVal > 0)
            return uVal;

        var tags = proto.Tags?.Select(t => t.ToLowerInvariant()).ToList() ?? new System.Collections.Generic.List<string>();
        var name = proto.Name?.ToLowerInvariant() ?? "";
        var unit = proto.Unit?.ToLowerInvariant() ?? "";

        if (tags.Contains("такси") || tags.Contains("маршрут") || name.Contains("такси") || name.Contains("маршрут"))
            return 8; // LocationBased
        if (tags.Contains("доставка") || name.Contains("доставка"))
            return 7; // DeliveryScheduled
        if (tags.Contains("отель") || tags.Contains("гостиница") || name.Contains("отель") || name.Contains("номер"))
            return 5; // HotelAccommodation
        if (tags.Contains("авиабилеты") || tags.Contains("билет") || tags.Contains("мероприятия") || name.Contains("билет") || name.Contains("концерт") || name.Contains("кино"))
            return 4; // TicketBooking
        if (tags.Contains("красота") || tags.Contains("врач") || tags.Contains("консультация") || tags.Contains("массаж") || name.Contains("консультация") || name.Contains("стрижка") || name.Contains("массаж"))
            return 3; // BookableResource
        if (tags.Contains("аренда авто") || tags.Contains("аренда") || name.Contains("аренда"))
            return 6; // VehicleRental
        if (tags.Contains("фитнес") || tags.Contains("подписка") || unit == "месяц" || unit == "год")
            return 9; // Subscription
        if (tags.Contains("подарки") || proto.Type == 6) // GiftCard
            return 13; // GiftCertificate
        if (tags.Contains("аукцион") || name.Contains("аукцион"))
            return 15; // BiddingAuction
        if (tags.Contains("комбо") || tags.Contains("закупки") || name.Contains("комбо"))
            return 23; // GroupBuy
        if (tags.Contains("ремонт") || tags.Contains("смета") || tags.Contains("rfq") || name.Contains("смет"))
            return 14; // PriceQuoteRequest
        if (tags.Contains("донаты") || tags.Contains("благотворительность") || name.Contains("взнос") || tags.Contains("баланс"))
            return 12; // Donation
        if (proto.Type == 4 || proto.Type == 5 || proto.Type == 7 || proto.Type == 8) // PromoCode, Discount, Money, Percent
            return 10; // DigitalGoods

        return 1; // CatalogItem
    }
}
