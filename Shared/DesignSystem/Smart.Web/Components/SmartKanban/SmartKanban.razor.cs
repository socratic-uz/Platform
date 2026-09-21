using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Smart.Web.Components;

public class KanbanColumn
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public object? StatusValue { get; set; }
}

public class CardMoveEventArgs<TItem>
{
    public TItem Item { get; set; } = default!;
    public KanbanColumn TargetColumn { get; set; } = default!;
}

public partial class SmartKanban<TItem> : ComponentBase where TItem : class
{
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public List<KanbanColumn>? Columns { get; set; }
    [Parameter] public Func<TItem, object?>? StatusSelector { get; set; }
    [Parameter] public Func<TItem, string>? TitleSelector { get; set; }
    [Parameter] public Func<TItem, string>? SubtitleSelector { get; set; }
    [Parameter] public RenderFragment<TItem>? CardTemplate { get; set; }
    [Parameter] public EventCallback<TItem> OnCardClicked { get; set; }
    [Parameter] public EventCallback<CardMoveEventArgs<TItem>> OnCardMoved { get; set; }

    private TItem? _draggedItem;

    private static readonly Func<TItem, object?>? CachedStatusGetter;
    private static readonly Action<TItem, object?>? CachedStatusSetter;
    private static readonly Func<TItem, string>? CachedTitleGetter;
    private static readonly Func<TItem, string>? CachedSubtitleGetter;

    static SmartKanban()
    {
        var props = typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        var statusProp = props.FirstOrDefault(p => p.Name.Equals("Status", StringComparison.OrdinalIgnoreCase));
        if (statusProp != null && statusProp.CanRead)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TItem), "x");
            var body = System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Property(param, statusProp), typeof(object));
            CachedStatusGetter = System.Linq.Expressions.Expression.Lambda<Func<TItem, object?>>(body, param).Compile();

            if (statusProp.CanWrite)
            {
                try
                {
                    var valParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "val");
                    var targetType = Nullable.GetUnderlyingType(statusProp.PropertyType) ?? statusProp.PropertyType;
                    var conv = System.Linq.Expressions.Expression.Convert(valParam, targetType);
                    var assign = System.Linq.Expressions.Expression.Call(param, statusProp.GetSetMethod()!, conv);
                    CachedStatusSetter = System.Linq.Expressions.Expression.Lambda<Action<TItem, object?>>(assign, param, valParam).Compile();
                }
                catch { }
            }
        }

        var titleProp = props.FirstOrDefault(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Title", StringComparison.OrdinalIgnoreCase));
        if (titleProp != null && titleProp.CanRead)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TItem), "x");
            var propAccess = System.Linq.Expressions.Expression.Property(param, titleProp);
            var toStr = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(propAccess, typeof(object)), typeof(object).GetMethod("ToString")!);
            CachedTitleGetter = System.Linq.Expressions.Expression.Lambda<Func<TItem, string>>(toStr, param).Compile();
        }

        var subProp = props.FirstOrDefault(p => p.Name.Equals("Description", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Status", StringComparison.OrdinalIgnoreCase));
        if (subProp != null && subProp.CanRead)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TItem), "x");
            var propAccess = System.Linq.Expressions.Expression.Property(param, subProp);
            var toStr = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(propAccess, typeof(object)), typeof(object).GetMethod("ToString")!);
            CachedSubtitleGetter = System.Linq.Expressions.Expression.Lambda<Func<TItem, string>>(toStr, param).Compile();
        }
    }

    private List<TItem> GetItemsForColumn(KanbanColumn col)
    {
        if (Items == null) return new List<TItem>();

        var getter = StatusSelector ?? CachedStatusGetter;
        if (getter != null)
        {
            return Items.Where(item => Equals(getter(item), col.StatusValue)).ToList();
        }

        return Items.ToList();
    }

    private void HandleDragStart(TItem item)
    {
        _draggedItem = item;
    }

    private async Task HandleDrop(KanbanColumn col)
    {
        if (_draggedItem != null)
        {
            var args = new CardMoveEventArgs<TItem>
            {
                Item = _draggedItem,
                TargetColumn = col
            };

            if (CachedStatusSetter != null && col.StatusValue != null)
            {
                try
                {
                    CachedStatusSetter(_draggedItem, col.StatusValue);
                }
                catch { }
            }

            await OnCardMoved.InvokeAsync(args);
            _draggedItem = null;
            StateHasChanged();
        }
    }

    private async Task HandleCardClick(TItem item)
    {
        await OnCardClicked.InvokeAsync(item);
    }

    private string GetItemTitle(TItem item)
    {
        if (TitleSelector != null) return TitleSelector(item);
        if (CachedTitleGetter != null) return CachedTitleGetter(item);
        return item.ToString() ?? string.Empty;
    }

    private string GetItemSubtitle(TItem item)
    {
        if (SubtitleSelector != null) return SubtitleSelector(item);
        if (CachedSubtitleGetter != null) return CachedSubtitleGetter(item);
        return string.Empty;
    }
}
