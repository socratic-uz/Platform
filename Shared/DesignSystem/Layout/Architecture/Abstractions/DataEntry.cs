using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using SharedKernel.Extensions;

namespace Domain.Abstractions
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Внутреннее (нетипизированное) хранилище одного источника данных.
    //  Не светится наружу — снаружи работа идёт только через типизированные
    //  методы MultiDataComponentBase.
    // ──────────────────────────────────────────────────────────────────────────

    internal abstract class DataEntry
    {
        public Type DtoType { get; protected init; }
        public abstract Task LoadAsync();
        public abstract bool TryRestore(PersistentComponentState state);
        public abstract void Persist(PersistentComponentState state);
    }

    internal sealed class DataEntry<TDto> : DataEntry
    {
        private readonly IServiceClient<TDto> _client;

        public List<TDto> Data { get; } = new();
        public TDto Query { get; set; }

        public DataEntry(IServiceClient<TDto> client, TDto query = default)
        {
            DtoType = typeof(TDto);
            _client = client;
            Query = query;
        }

        public override async Task LoadAsync()
        {
            Data.Clear();
            Data.AddRange(await _client.Read(Query));
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "DTO types for DataEntry are explicitly preserved in the AOT Json Context")]
        public override bool TryRestore(PersistentComponentState state)
        {
            var key = $"DataComponentBase_{DtoType.FullName}";
            if (typeof(Google.Protobuf.IMessage).IsAssignableFrom(typeof(TDto)))
            {
                if (state.TryTakeProtobufListReflection(key, typeof(TDto), out var restoredList))
                {
                    Data.Clear();
                    if (restoredList != null)
                    {
                        foreach (var item in restoredList)
                        {
                            Data.Add((TDto)item);
                        }
                    }
                    return true;
                }
            }
            else
            {
                if (state.TryTakeFromJson<List<TDto>>(key, out var restoredData))
                {
                    Data.Clear();
                    if (restoredData != null)
                    {
                        Data.AddRange(restoredData);
                    }
                    return true;
                }
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "DTO types for DataEntry are explicitly preserved in the AOT Json Context")]
        public override void Persist(PersistentComponentState state)
        {
            var key = $"DataComponentBase_{DtoType.FullName}";
            if (typeof(Google.Protobuf.IMessage).IsAssignableFrom(typeof(TDto)))
            {
                state.PersistProtobufList(key, Data.Cast<Google.Protobuf.IMessage>());
            }
            else
            {
                state.PersistAsJson(key, Data);
            }
        }
    }
}
