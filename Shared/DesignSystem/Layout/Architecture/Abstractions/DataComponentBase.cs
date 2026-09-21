using Microsoft.AspNetCore.Components;

namespace Domain.Abstractions
{
    // ──────────────────────────────────────────────────────────────────────────
    //  MultiDataComponentBase: несколько клиентов, разные DTO, ключ — Type.
    // ──────────────────────────────────────────────────────────────────────────

    public class DataComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        public IServiceClientFactory ClientFactory { get; set; }

        [Inject]
        public PersistentComponentState ApplicationState { get; set; }

        private PersistingComponentStateSubscription _subscription;

        private readonly Dictionary<Type, DataEntry> _entries = new();

        public string? ErrorMessage { get; protected set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Регистрирует источник данных для TDto. Клиент берётся из фабрики.
        /// Вызывается в OnInitialized наследником (или переопределите
        /// RegisterDataSources(), см. ниже) до того, как данные будут загружены.
        /// </summary>
        public void Register<TDto>(TDto query = default)
        {
            var dtoType = typeof(TDto);
            if (_entries.ContainsKey(dtoType))
                return; // уже зарегистрирован — не перезатираем

            var client = ClientFactory.GetClient<TDto>();
            _entries[dtoType] = new DataEntry<TDto>(client, query);
        }

        /// <summary>
        /// Точка расширения: наследник перечисляет нужные ему DTO.
        /// Пример (Query — того же типа, что и DTO, как в IServiceClient&lt;T&gt;):
        /// protected override void RegisterDataSources()
        /// {
        ///     Register&lt;UserDto&gt;();
        ///     Register&lt;OrderDto&gt;(new OrderDto { Status = "Active" });
        /// }
        /// </summary>
        protected virtual void RegisterDataSources()
        {
        }

        public List<TDto> Data<TDto>()
        {
            if (_entries.TryGetValue(typeof(TDto), out var entry) && entry is DataEntry<TDto> typed)
                return typed.Data;

            throw new InvalidOperationException($"Источник данных для типа {typeof(TDto).Name} не зарегистрирован. " + $"Вызовите Register<{typeof(TDto).Name}>() в RegisterDataSources().");
        }

        public TDto Query<TDto>()
        {
            if (_entries.TryGetValue(typeof(TDto), out var entry) && entry is DataEntry<TDto> typed)
                return typed.Query;

            throw new InvalidOperationException(
                $"Источник данных для типа {typeof(TDto).Name} не зарегистрирован.");
        }

        public void SetQuery<TDto>(TDto query)
        {
            if (_entries.TryGetValue(typeof(TDto), out var entry) && entry is DataEntry<TDto> typed)
            {
                typed.Query = query;
                return;
            }

            throw new InvalidOperationException($"Источник данных для типа {typeof(TDto).Name} не зарегистрирован.");
        }

        /// <summary>
        /// Перезагружает один источник данных (например, после изменения Query).
        /// </summary>
        protected async Task ReloadAsync<TDto>()
        {
            if (_entries.TryGetValue(typeof(TDto), out var entry))
                await entry.LoadAsync();
        }

        /// <summary>
        /// Перезагружает все источники данных.
        /// </summary>
        protected async Task ReloadAllAsync()
        {
            foreach (var entry in _entries.Values)
                await entry.LoadAsync();
        }

        public bool IsLoading { get; protected set; } = true;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            RegisterDataSources();

            try
            {
                _subscription = ApplicationState.RegisterOnPersisting(PersistData);
            }
            catch { }

            var loadTasks = new List<Task>();
            foreach (var entry in _entries.Values)
            {
                bool restored = false;
                try
                {
                    restored = entry.TryRestore(ApplicationState);
                }
                catch { }

                if (!restored)
                {
                    loadTasks.Add(entry.LoadAsync());
                }
            }

            if (loadTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(loadTasks);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    System.Diagnostics.Debug.WriteLine($"[DataComponentBase] Exception in Task.WhenAll: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else
            {
                IsLoading = false;
            }

            await base.OnInitializedAsync();
        }

        private Task PersistData()
        {
            foreach (var entry in _entries.Values)
            {
                entry.Persist(ApplicationState);
            }
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            try
            {
                _subscription.Dispose();
            }
            catch { }
        }
    }
}
