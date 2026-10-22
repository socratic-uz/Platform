using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Assets.Constants;

using SystemEnum = System.Enum;
using SystemType = System.Type;

namespace Smart.Web.Components;

public partial class SmartForm<TItem> : ComponentBase where TItem : class, new()
{
    [Inject] public IStringLocalizer L { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    protected string EffectiveCancelText => !string.IsNullOrWhiteSpace(CancelText) && CancelText != "Cancel" && CancelText != "Отмена"
        ? CancelText 
        : (L != null ? L[Loc.Actions.CancelAction] : "Cancel");

    protected string EffectiveSubmitText => !string.IsNullOrWhiteSpace(SubmitText) && SubmitText != "Submit" && SubmitText != "Сохранить"
        ? SubmitText 
        : (L != null ? L[Loc.Actions.SaveAction] : "Save");

    protected string formId = Guid.NewGuid().ToString();
    protected PropertyInfo[]? propertyInfos;
    protected PropertyInfo? imagesProperty;
    protected PropertyInfo? newImagesContentProperty;
    protected PropertyInfo? newImagesFileNameProperty;
    protected PropertyInfo? newImagesContentTypeProperty;

    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public bool Open { get; set; } = false;
    [Parameter] public bool Quick { get; set; } = false;
    [Parameter] public string Type { get; set; } = "alert";
    [Parameter] public bool NoFocusTrap { get; set; } = false;
    [Parameter] public string AriaLabel { get; set; } = "";
    [Parameter] public EventCallback<EventArgs> OnClose { get; set; }
    [Parameter] public EventCallback<EventArgs> OnCancel { get; set; }
    [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }
    [Parameter] public EventCallback<EditContext> OnInvalidSubmit { get; set; }

    [Parameter] public RenderFragment? Headline { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }
    [Parameter] public RenderFragment? AdditionalFields { get; set; }

    [Parameter] public TItem Item { get; set; } = new();

    // Новые параметры для улучшенной функциональности
    [Parameter] public bool ShowValidationSummary { get; set; } = true;
    [Parameter] public string SubmitText { get; set; } = "Submit";
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public List<string> ExcludeProperties { get; set; } = new();
    [Parameter] public Dictionary<string, string> PropertyDisplayNames { get; set; } = new();
    [Parameter] public Dictionary<string, string> PropertyTypes { get; set; } = new();
    [Parameter] public bool UseCheckboxForBool { get; set; } = false;
    [Parameter] public bool UseChipsForEnum { get; set; } = true;
    [Parameter] public bool UseChipSetForEnum { get; set; } = true;
    [Parameter] public bool UseSmartCheckboxForEnum { get; set; } = false;
    [Parameter] public Func<string, string>? ImageUrlResolver { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    protected EditContext? editContext;

    protected override void OnParametersSet()
    {
        if (editContext == null || editContext.Model != Item)
        {
            editContext = new EditContext(Item);
        }
        base.OnParametersSet();
    }

    protected async Task SubmitFormAsync()
    {
        editContext ??= new EditContext(Item);
        bool isValid = editContext.Validate();
        if (isValid)
        {
            await OnValidSubmit.InvokeAsync(editContext);
        }
        else
        {
            await OnInvalidSubmit.InvokeAsync(editContext);
        }
    }

    protected override Task OnInitializedAsync()
    {
        var allProps = typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        imagesProperty = allProps.FirstOrDefault(p => p.Name.Equals("Images", StringComparison.OrdinalIgnoreCase));
        newImagesContentProperty = allProps.FirstOrDefault(p => p.Name.Equals("NewImagesContent", StringComparison.OrdinalIgnoreCase));
        newImagesFileNameProperty = allProps.FirstOrDefault(p => p.Name.Equals("NewImagesFileName", StringComparison.OrdinalIgnoreCase));
        newImagesContentTypeProperty = allProps.FirstOrDefault(p => p.Name.Equals("NewImagesContentType", StringComparison.OrdinalIgnoreCase));

        var imageRelatedNames = new List<string>();
        if (imagesProperty != null && newImagesContentProperty != null)
        {
            imageRelatedNames.Add(newImagesContentProperty.Name);
            if (newImagesFileNameProperty != null) imageRelatedNames.Add(newImagesFileNameProperty.Name);
            if (newImagesContentTypeProperty != null) imageRelatedNames.Add(newImagesContentTypeProperty.Name);
        }

        propertyInfos = allProps
            .Where(p =>
                p.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold != false &&
                ((p.CanRead && p.CanWrite) ||
                 (imagesProperty != null && p.Name.Equals(imagesProperty.Name, StringComparison.OrdinalIgnoreCase)) ||
                 IsFileCollection(p)) &&
                !ExcludeProperties.Contains(p.Name) &&
                !imageRelatedNames.Contains(p.Name) &&
                !(p.Name.StartsWith("Has") && p.PropertyType == typeof(bool) && typeof(TItem).GetProperty(p.Name.Substring(3)) != null))
            .ToArray();

        return Task.CompletedTask;
    }

    protected bool IsFileCollection(PropertyInfo prop)
    {
        var type = prop.PropertyType;
        if (!type.IsGenericType) return false;

        var genericArgs = type.GetGenericArguments();
        if (genericArgs.Length == 1 && (genericArgs[0].FullName == "SharedKernel.Protos.File" || genericArgs[0].FullName == "Core.Domain.Entities.File"))
        {
            return true;
        }

        var iListInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
        if (iListInterface != null)
        {
            var ifaceArgs = iListInterface.GetGenericArguments();
            if (ifaceArgs.Length == 1 && (ifaceArgs[0].FullName == "SharedKernel.Protos.File" || ifaceArgs[0].FullName == "Core.Domain.Entities.File"))
            {
                return true;
            }
        }

        return false;
    }

    protected bool IsSimpleType(SystemType type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(Guid) ||
               type.IsEnum;
    }

    protected string GetDisplayName(PropertyInfo property)
    {
        if (PropertyDisplayNames.TryGetValue(property.Name, out var customName))
            return customName;

        var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? property.Name;
    }

    protected bool IsRequired(PropertyInfo property)
    {
        return property.GetCustomAttribute<RequiredAttribute>() != null;
    }

    protected string GetFieldType(PropertyInfo property)
    {
        if (PropertyTypes.ContainsKey(property.Name))
            return PropertyTypes[property.Name];

        var dataTypeAttribute = property.GetCustomAttribute<DataTypeAttribute>();
        if (dataTypeAttribute != null)
        {
            return dataTypeAttribute.DataType switch
            {
                DataType.Password => "password",
                DataType.EmailAddress => "email",
                DataType.MultilineText => "textarea",
                DataType.PhoneNumber => "tel",
                DataType.Url => "url",
                DataType.Date => "date",
                DataType.DateTime => "datetime-local",
                DataType.Time => "time",
                DataType.Currency => "number",
                DataType.Text => "text",
                DataType.ImageUrl => "text",
                DataType.Upload => "file",
                _ => "text"
            };
        }

        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType == typeof(int) || propertyType == typeof(long) ||
            propertyType == typeof(decimal) || propertyType == typeof(double) ||
            propertyType == typeof(float))
        {
            return "number";
        }

        if (propertyType == typeof(DateTime))
        {
            var propertyName = property.Name.ToLower();
            if (propertyName.Contains("date") && !propertyName.Contains("time"))
                return "date";
            if (propertyName.Contains("time") && !propertyName.Contains("date"))
                return "time";
            return "datetime-local";
        }

        if (propertyType == typeof(bool))
            return "switch";

        if (property.Name.ToLower().Contains("email"))
            return "email";

        if (property.Name.ToLower().Contains("password"))
            return "password";

        if (property.Name.ToLower().Contains("description") ||
            property.Name.ToLower().Contains("comment") ||
            property.Name.ToLower().Contains("note"))
            return "textarea";

        return "text";
    }

    protected int GetTextAreaRows(PropertyInfo property)
    {
        var stringLengthAttribute = property.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttribute != null && stringLengthAttribute.MaximumLength > 100)
            return 4;
        return 2;
    }

    protected string GetEnumDisplayText(object enumValue, SystemType enumType)
    {
        var memberInfo = enumType.GetMember(enumValue.ToString() ?? "").FirstOrDefault();
        if (memberInfo != null)
        {
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute?.Name != null)
                return displayAttribute.Name;

            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute?.Description != null)
                return descriptionAttribute.Description;
        }

        return enumValue.ToString() ?? "";
    }

    protected bool IsZoneField(PropertyInfo info, SystemType propertyType)
    {
        if (propertyType.Name.Contains("Zone", StringComparison.OrdinalIgnoreCase) || propertyType.Name.Contains("Polygon", StringComparison.OrdinalIgnoreCase))
            return true;

        if (propertyType.IsGenericType)
        {
            var genericArgs = propertyType.GetGenericArguments();
            if (genericArgs.Length == 1 && (genericArgs[0].Name.Contains("Zone", StringComparison.OrdinalIgnoreCase) || genericArgs[0].Name.Contains("Polygon", StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        return false;
    }

    protected string ResolveImageUrl(string image)
    {
        if (ImageUrlResolver != null)
        {
            return ImageUrlResolver(image);
        }

        var typeName = typeof(TItem).Name.ToLower();
        if (typeName.Contains("organization") || typeName.Contains("org"))
        {
            return BuildDefaultImageUrl(image, "org");
        }
        else if (typeName.Contains("product"))
        {
            return BuildDefaultImageUrl(image, "product");
        }
        else if (typeName.Contains("profile") || typeName.Contains("user"))
        {
            return BuildDefaultImageUrl(image, "profile");
        }

        return BuildDefaultImageUrl(image, "general");
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Dynamic reflection lookup for FileHelper is preserved for AOT UI fallback")]
    protected string BuildDefaultImageUrl(string image, string category)
    {
        var fileHelperType = typeof(TItem).Assembly.GetType("Core.Shared.Helpers.FileHelper") ??
                             AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => {
                                    try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; }
                                })
                                .FirstOrDefault(t => t.FullName == "Core.Shared.Helpers.FileHelper");

        if (fileHelperType != null)
        {
            var minIoUrlProp = fileHelperType.GetProperty("MinIOUrl", BindingFlags.Public | BindingFlags.Static);
            var minIoUrl = minIoUrlProp?.GetValue(null)?.ToString();
            if (!string.IsNullOrEmpty(minIoUrl))
            {
                if (string.IsNullOrWhiteSpace(image))
                {
                    return $"{minIoUrl}/img/{category}/default.png";
                }
                if (Uri.TryCreate(image, UriKind.Absolute, out _))
                {
                    return image;
                }
                var normalized = image.TrimStart('/');
                if (normalized.StartsWith("img/", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{minIoUrl}/{normalized}";
                }
                if (normalized.Contains('/'))
                {
                    return $"{minIoUrl}/img/{normalized}";
                }
                return $"{minIoUrl}/img/{category}/{normalized}";
            }
        }

        return image;
    }

    protected static class EnumCallbackHelper
    {
        public static object CreateCallback<TEnum>(object receiver, PropertyInfo property, object parentObj, Action stateHasChanged) where TEnum : struct, Enum
        {
            return EventCallback.Factory.Create<TEnum>(receiver, new Action<TEnum>((TEnum value) =>
            {
                property.SetValue(parentObj, value);
                stateHasChanged();
            }));
        }

        public static System.Linq.Expressions.Expression<Func<TEnum>> CreateExpression<TEnum>(PropertyInfo property, object parentObj)
        {
            var constantExpr = System.Linq.Expressions.Expression.Constant(parentObj);
            var memberExpr = System.Linq.Expressions.Expression.Property(constantExpr, property);
            return System.Linq.Expressions.Expression.Lambda<Func<TEnum>>(memberExpr);
        }
    }
}
