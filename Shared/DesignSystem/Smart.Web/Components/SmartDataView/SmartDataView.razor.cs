using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Smart.Web.Components;

public enum ViewMode
{
    Grid,
    Kanban
}

public partial class SmartDataView<TItem> : ComponentBase where TItem : class, new()
{
    [Parameter] public IQueryable<TItem>? Items { get; set; }
    [Parameter] public int PageSize { get; set; } = 20;
    [Parameter] public ViewMode CurrentMode { get; set; } = ViewMode.Grid;
    [Parameter] public EventCallback<ViewMode> CurrentModeChanged { get; set; }

    // Filter Parameters
    [Parameter] public string FilterPlaceholder { get; set; } = "Поиск элементов...";
    [Parameter] public string SearchText { get; set; } = string.Empty;
    [Parameter] public List<FilterChip>? FilterChips { get; set; }

    // Kanban Parameters
    [Parameter] public List<KanbanColumn>? KanbanColumns { get; set; }
    [Parameter] public Func<TItem, object?>? StatusSelector { get; set; }
    [Parameter] public Func<TItem, string>? TitleSelector { get; set; }
    [Parameter] public Func<TItem, string>? SubtitleSelector { get; set; }

    // Callbacks
    [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }
    [Parameter] public EventCallback<TItem> OnCardClicked { get; set; }
    [Parameter] public EventCallback<CardMoveEventArgs<TItem>> OnCardMoved { get; set; }

    private static readonly Func<TItem, string?>[] CachedStringGetters = typeof(TItem)
        .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        .Where(p => p.PropertyType == typeof(string) && p.CanRead)
        .Select(p =>
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TItem), "x");
            var body = System.Linq.Expressions.Expression.Property(param, p);
            return System.Linq.Expressions.Expression.Lambda<Func<TItem, string?>>(body, param).Compile();
        })
        .ToArray();

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "In-memory collection filtering uses static getters")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "In-memory collection filtering uses static getters")]
    protected virtual IQueryable<TItem>? FilteredItems
    {
        get
        {
            if (Items == null) return null;
            if (string.IsNullOrWhiteSpace(SearchText)) return Items;

            var lower = SearchText.ToLowerInvariant();
            var getters = CachedStringGetters;
            if (getters.Length == 0) return Items;

            return Items.AsEnumerable().Where(item =>
                getters.Any(getter =>
                {
                    var val = getter(item);
                    return val != null && val.Contains(lower, StringComparison.OrdinalIgnoreCase);
                })
            ).AsQueryable();
        }
    }

    private async Task SetViewMode(ViewMode mode)
    {
        CurrentMode = mode;
        await CurrentModeChanged.InvokeAsync(mode);
    }

    private void OnSearchTextChanged(string text)
    {
        SearchText = text;
        StateHasChanged();
    }

    private void OnChipToggled(FilterChip chip)
    {
        StateHasChanged();
    }
}
