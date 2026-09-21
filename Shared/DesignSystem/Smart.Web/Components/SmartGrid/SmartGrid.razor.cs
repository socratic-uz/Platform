using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

using QuickGrid;
using System.Linq;


using SharedKernel.Abstractions;
using SortDirection = QuickGrid.SortDirection;

namespace Smart.Web.Components
{
    public partial class SmartGrid<TItem> : QuickGrid<TItem>
    {
        TItem currentModel = new();
        bool isFormOpen = false;
        string dialogTitle = "";
        string dialogIcon = "";
        bool isCreateMode = false;
        string submitButtonText = "";
        PropertyInfo[] allProperties = Array.Empty<PropertyInfo>();
        List<ColumnInfo<TItem>> displayColumns = new();

        [Inject] IStringLocalizer L { get; set; } = default!;
        [Inject] IServiceProvider ServiceProvider { get; set; } = default!;

        private void OpenCreateDialog()
        {
            currentModel = new();
            dialogTitle = "Создание записи";
            dialogIcon = "add";
            submitButtonText = "Создать";
            isCreateMode = true;
            isFormOpen = true;
        }

        private void OpenEditDialog(TItem item)
        {
            currentModel = item;
            dialogTitle = "Редактирование записи";
            dialogIcon = "edit";
            submitButtonText = "Сохранить";
            isCreateMode = false;
            isFormOpen = true;
        }

        private async Task HandleValidSubmit(EditContext context)
        {
            isFormOpen = false;

            if (ServiceProvider != null)
            {
                try
                {
                    var client = ServiceProvider.GetService(typeof(IServiceClient<TItem>)) as IServiceClient<TItem>;
                    if (client != null)
                    {
                        if (isCreateMode)
                        {
                            await client.Create(currentModel);
                        }
                        else
                        {
                            await client.Update(currentModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartGrid] Backend call error: {ex.Message}");
                }
            }

            await OnValidSubmit.InvokeAsync(context);
            StateHasChanged();
        }

        // Dialog Methods
        [Parameter] public EventCallback<MouseEventArgs> CreateEvent { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> EditEvent { get; set; }
        // OnValidSubmit
        [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }
        [Parameter] public EventCallback<EditContext> OnInvalidSubmit { get; set; }

        // Grid Configuration
        [Parameter] public List<string> ExcludeColumns { get; set; } = new();
        [Parameter] public List<string> IncludeOnlyColumns { get; set; } = new();
        [Parameter] public Dictionary<string, string> ColumnDisplayNames { get; set; } = new();
        [Parameter] public Dictionary<string, string> ColumnFormats { get; set; } = new();
        [Parameter] public Dictionary<string, bool> ColumnSortable { get; set; } = new();
        [Parameter] public string? DefaultSortColumn { get; set; }
        [Parameter] public SortDirection DefaultSortDirection { get; set; } = SortDirection.Ascending;

        // Form Configuration
        [Parameter] public List<string> ExcludeProperties { get; set; } = new();
        [Parameter] public Dictionary<string, string> PropertyDisplayNames { get; set; } = new();
        [Parameter] public Dictionary<string, string> PropertyTypes { get; set; } = new();

        // UI Configuration
        [Parameter] public int? PageSize { get; set; }
        [Parameter] public bool AsCards { get; set; } = false;


        protected override void OnInitialized()
        {
            InitializeColumns();
            base.OnInitialized();
        }


        protected override async Task OnParametersSetAsync()
        {
            var effectivePageSize = PageSize ?? (AsCards ? 0 : 20);
            if (effectivePageSize > 0)
            {
                Pagination ??= new PaginationState();
                Pagination.ItemsPerPage = effectivePageSize;
            }
            else
            {
                Pagination = null;
            }

            InitializeColumns();
            await base.OnParametersSetAsync();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }

        private void InitializeColumns()
        {
            allProperties = typeof(TItem)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Where(p => {
                    var scaffold = p.GetCustomAttribute<ScaffoldColumnAttribute>();
                    if (scaffold != null && !scaffold.Scaffold) return false;

                    var display = p.GetCustomAttribute<DisplayAttribute>();
                    if (display != null && display.GetAutoGenerateField() == false) return false;

                    return true;
                })
                .Where(p => !(p.Name.StartsWith("Has") && p.PropertyType == typeof(bool) && typeof(TItem).GetProperty(p.Name.Substring(3)) != null))
                .ToArray();

            displayColumns = GetDisplayColumns().ToList();
        }


        private static readonly System.Collections.Concurrent.ConcurrentDictionary<PropertyInfo, Func<TItem, object?>> GetterCache = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<PropertyInfo, GridSort<TItem>> SortRuleCache = new();

        private static Func<TItem, object?> GetOrCreateGetter(PropertyInfo property)
        {
            return GetterCache.GetOrAdd(property, p => {
                var param = Expression.Parameter(typeof(TItem), "item");
                var propAccess = Expression.Property(param, p);
                var convert = Expression.Convert(propAccess, typeof(object));
                return Expression.Lambda<Func<TItem, object?>>(convert, param).Compile();
            });
        }

        private IEnumerable<ColumnInfo<TItem>> GetDisplayColumns()
        {
            var properties = allProperties;

            // Применяем фильтры включения/исключения
            if (IncludeOnlyColumns.Any())
            {
                properties = properties.Where(p => IncludeOnlyColumns.Contains(p.Name)).ToArray();
            }

            if (ExcludeColumns.Any())
            {
                properties = properties.Where(p => !ExcludeColumns.Contains(p.Name)).ToArray();
            }

            return properties.Select(p => new ColumnInfo<TItem>
            {
                Property = p,
                DisplayName = GetColumnDisplayName(p),
                IsSortable = GetColumnSortable(p),
                Format = ColumnFormats.TryGetValue(p.Name, out var format) ? format : null,
                IsDefaultSort = p.Name == DefaultSortColumn,
                Getter = GetOrCreateGetter(p),
                SortRule = GetColumnSortable(p) ? GetSortRule(p) : null
            }).ToList();
        }

        private bool CanCreatePropertyColumn(Type propertyType)
        {
            // Treat only simple/scalar types as property columns; enums are handled separately
            return propertyType.IsPrimitive ||
                   propertyType == typeof(string) ||
                   propertyType == typeof(DateTime) ||
                   propertyType == typeof(DateOnly) ||
                   propertyType == typeof(TimeOnly) ||
                   propertyType == typeof(Guid) ||
                   propertyType == typeof(decimal) ||
                   // Добавляем поддержку ObjectId (MongoDB) как скалярного
                   propertyType.Name == "ObjectId";
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "TypeDescriptor fallback formatting is safe for preserved primitive grid values")]
        private string FormatValue(object? value, string? format)
        {
            if (value == null) return "";

            if (!string.IsNullOrEmpty(format))
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(format, null);
                }
            }

            if (value is not string && value is System.Collections.IEnumerable)
            {
                // Let the switch statement format the collection elements
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(value.GetType());
                if (converter != null && converter.CanConvertTo(typeof(string)) && converter.GetType() != typeof(TypeConverter))
                {
                    return converter.ConvertToString(value) ?? "";
                }
            }

            return value switch
            {
                DateTime dt => dt.ToString("dd.MM.yyyy HH:mm"),
                DateOnly d => d.ToString("dd.MM.yyyy"),
                TimeOnly t => t.ToString("HH:mm"),
                decimal dec => dec.ToString("F2"),
                double dbl => dbl.ToString("F2"),
                float flt => flt.ToString("F2"),
                bool b => b ? "Да" : "Нет",
                Enum e => e.ToString(),
                SharedKernel.ValueObjects.WorkSchedule ws => FormatWorkSchedule(ws),
                TimeSpan ts => ts.ToString(@"hh\:mm"),
                // Обработка ObjectId
                var objId when objId.GetType().Name == "ObjectId" => objId.ToString() ?? "",
                // Обработка Dictionary
                System.Collections.IDictionary dict => FormatDictionary(dict),
                // Обработка Address
                var addr when addr.GetType().Name == "Address" => FormatAddress(addr),
                System.Collections.IEnumerable enumerable when value is not string =>
                    string.Join(", ", enumerable.Cast<object>().Take(3).Select(x => x.ToString())) + (enumerable.Cast<object>().Count() > 3 ? ", ..." : ""),
                _ => value.ToString() ?? ""
            };
        }

        private string FormatWorkSchedule(SharedKernel.ValueObjects.WorkSchedule ws)
        {
            if (ws == null || ws.WeeklyItems == null || ws.WeeklyItems.Count == 0)
                return "Не задан";

            var activeItems = ws.WeeklyItems.Where(w => !w.IsClosed && w.Intervals != null && w.Intervals.Count > 0).ToList();
            if (!activeItems.Any()) return "Выходные";

            var first = activeItems.First().Intervals.First();
            return $"{activeItems.Count} дн. ({first.Start:hh\\:mm}-{first.End:hh\\:mm})";
        }

        private string FormatDictionary(IDictionary dict)
        {
            try
            {
                var items = new List<string>();
                foreach (DictionaryEntry entry in dict)
                {
                    items.Add($"{entry.Key}: {entry.Value}");
                }
                return string.Join("; ", items.Take(2)) + (items.Count > 2 ? "; ..." : "");
            }
            catch
            {
                return dict.ToString() ?? "";
            }
        }

        private string FormatAddress(object address)
        {
            try
            {
                // Используем рефлексию для вызова ToString с параметром
                var toStringMethod = address.GetType().GetMethod("ToString", new[] { typeof(string) });
                if (toStringMethod != null)
                {
                    return toStringMethod.Invoke(address, new object[] { ", " })?.ToString() ?? "";
                }
                return address.ToString() ?? "";
            }
            catch
            {
                return address.ToString() ?? "";
            }
        }

        private string GetColumnDisplayName(PropertyInfo property)
        {
            if (ColumnDisplayNames.TryGetValue(property.Name, out var customName))
                return customName;

            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? property.Name;
        }

        private bool GetColumnSortable(PropertyInfo property)
        {
            if (ColumnSortable.TryGetValue(property.Name, out var sortable))
                return sortable;

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            return CanCreatePropertyColumn(propertyType) || propertyType.IsEnum;
        }

        private string GetEnumDisplayText(object enumValue, Type enumType)
        {
            // Получаем DisplayAttribute если есть
            var memberInfo = enumType.GetMember(enumValue.ToString() ?? "").FirstOrDefault();
            if (memberInfo != null)
            {
                var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute?.Name != null)
                    return displayAttribute.Name;

                var descriptionAttribute = memberInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                if (descriptionAttribute?.Description != null)
                    return descriptionAttribute.Description;
            }

            return enumValue.ToString() ?? "";
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "GridSort dynamic lambdas are instantiated for grid properties")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "GridSort dynamic lambdas are instantiated for grid properties")]
        protected GridSort<TItem> GetSortRule(PropertyInfo property)
        {
            return SortRuleCache.GetOrAdd(property, p => {
                var parameter = Expression.Parameter(typeof(TItem), "item");
                var propertyAccess = Expression.Property(parameter, p);
                var funcType = typeof(Func<,>).MakeGenericType(typeof(TItem), p.PropertyType);
                var lambda = Expression.Lambda(funcType, propertyAccess, parameter);

                var method = typeof(GridSort<TItem>)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m => m.Name == "ByAscending" && m.IsGenericMethod)
                    .MakeGenericMethod(p.PropertyType);

                return (GridSort<TItem>)method.Invoke(null, new object[] { lambda })!;
            });
        }

        protected override void RenderRow(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int rowIndex, TItem item)
        {
            if (AsCards)
            {
                foreach (var col in _columns)
                {
                    col.CellContent(__builder, item);
                }
            }
            else
            {
                base.RenderRow(__builder, rowIndex, item);
            }
        }

        protected override void RenderNonVirtualizedRows(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            if (AsCards)
            {
                var initialRowIndex = 2;
                var rowIndex = initialRowIndex;
                foreach (var item in _currentNonVirtualizedViewItems)
                {
                    RenderRow(__builder, rowIndex++, item);
                }
            }
            else
            {
                base.RenderNonVirtualizedRows(__builder);
            }
        }

        protected override void RenderPlaceholderRow(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, Microsoft.AspNetCore.Components.Web.Virtualization.PlaceholderContext placeholderContext)
        {
            if (AsCards)
            {
                // Для карточек не рендерим ячейки таблицы
            }
            else
            {
                base.RenderPlaceholderRow(__builder, placeholderContext);
            }
        }
    }

    public class ColumnInfo<TItem>
    {
        public PropertyInfo Property { get; set; } = default!;
        public string DisplayName { get; set; } = "";
        public bool IsSortable { get; set; } = true;
        public string? Format { get; set; }
        public bool IsDefaultSort { get; set; } = false;
        public Func<TItem, object?> Getter { get; set; } = default!;
        public GridSort<TItem>? SortRule { get; set; }
    }
}
