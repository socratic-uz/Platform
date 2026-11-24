#nullable enable
#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Domain.Entities;
using Google.Protobuf;
using Microsoft.AspNetCore.Components;
using Shopping.Application.Protos;

namespace Shared.Serialization;

public static class AotPersistentStateExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Primitive string only.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Primitive string only.")]
    private static void WriteRaw(PersistentComponentState state, string key, string value) =>
        state.PersistAsJson(key, value);

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Primitive string only.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Primitive string only.")]
    private static bool ReadRaw(PersistentComponentState state, string key, [NotNullWhen(true)] out string? value) =>
        state.TryTakeFromJson(key, out value) && !string.IsNullOrEmpty(value);

    public static void PersistAot<T>(this PersistentComponentState state, string key, T value, JsonTypeInfo<T> typeInfo) =>
        WriteRaw(state, key, JsonSerializer.Serialize(value, typeInfo));

    public static bool TryTakeAot<T>(this PersistentComponentState state, string key, JsonTypeInfo<T> typeInfo, [MaybeNullWhen(false)] out T value)
    {
        if (ReadRaw(state, key, out var json) && JsonSerializer.Deserialize(json, typeInfo) is { } parsed)
        {
            value = parsed;
            return true;
        }
        value = default;
        return false;
    }

    private static void PersistList<TEntity, TProto>(PersistentComponentState state, string key, IReadOnlyList<TEntity> items, Func<TEntity, TProto> toProto)
        where TProto : IMessage
    {
        var payload = new List<byte[]>(items.Count);
        for (int i = 0; i < items.Count; i++) payload.Add(toProto(items[i]).ToByteArray());
        state.PersistAot(key, payload, AotJsonContext.Default.ListByteArray);
    }

    private static bool TryTakeList<TEntity>(PersistentComponentState state, string key, Func<byte[], TEntity> fromBytes, [NotNullWhen(true)] out List<TEntity>? items)
    {
        if (state.TryTakeAot(key, AotJsonContext.Default.ListByteArray, out var payload) && payload is not null)
        {
            items = new List<TEntity>(payload.Count);
            for (int i = 0; i < payload.Count; i++) items.Add(fromBytes(payload[i]));
            return true;
        }
        items = null;
        return false;
    }

    public static void PersistOrganization(this PersistentComponentState s, string k, Organization v) =>
        WriteRaw(s, k, Convert.ToBase64String(((OrganizationDto)v).ToByteArray()));

    public static bool TryTakeOrganization(this PersistentComponentState s, string k, [NotNullWhen(true)] out Organization? v)
    {
        v = ReadRaw(s, k, out var b64) ? (Organization)OrganizationDto.Parser.ParseFrom(Convert.FromBase64String(b64)) : null;
        return v is not null;
    }

    public static void PersistOrganizations(this PersistentComponentState s, string k, IReadOnlyList<Organization> v) =>
        PersistList(s, k, v, static x => (OrganizationDto)x);

    public static bool TryTakeOrganizations(this PersistentComponentState s, string k, [NotNullWhen(true)] out List<Organization>? v) =>
        TryTakeList(s, k, static b => (Organization)OrganizationDto.Parser.ParseFrom(b), out v);

    public static void PersistProduct(this PersistentComponentState s, string k, Product v) =>
        WriteRaw(s, k, Convert.ToBase64String(((ProductDto)v).ToByteArray()));

    public static bool TryTakeProduct(this PersistentComponentState s, string k, [NotNullWhen(true)] out Product? v)
    {
        v = ReadRaw(s, k, out var b64) ? (Product)ProductDto.Parser.ParseFrom(Convert.FromBase64String(b64)) : null;
        return v is not null;
    }

    public static void PersistProducts(this PersistentComponentState s, string k, IReadOnlyList<Product> v) =>
        PersistList(s, k, v, static x => (ProductDto)x);

    public static bool TryTakeProducts(this PersistentComponentState s, string k, [NotNullWhen(true)] out List<Product>? v) =>
        TryTakeList(s, k, static b => (Product)ProductDto.Parser.ParseFrom(b), out v);
}
