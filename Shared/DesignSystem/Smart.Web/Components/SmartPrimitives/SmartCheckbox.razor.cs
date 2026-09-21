using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Smart.Web.Components;

public partial class SmartCheckbox<TEnum> : ComponentBase where TEnum : struct, Enum
{
    // Îñíîâíûå ïàðàìåòðû
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public TEnum? SelectedValue { get; set; }
    [Parameter] public IEnumerable<TEnum>? SelectedValues { get; set; }
    [Parameter] public bool ShowAsGroup { get; set; } = true;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool IsRequired { get; set; } = false;
    [Parameter] public bool UseVerticalLayout { get; set; } = true; // Äîáàâëÿåì ïàðàìåòð äëÿ âåðòèêàëüíîãî ðàñïîëîæåíèÿ
    
    // Íàñòðîéêè îòîáðàæåíèÿ
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? GroupTitle { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? GroupAriaLabel { get; set; }
    [Parameter] public string? ContainerClass { get; set; }
    
    // Íàñòðîéêè enum
    [Parameter] public Func<TEnum, string>? EnumTextSelector { get; set; }
    [Parameter] public Func<TEnum, string>? EnumIconSelector { get; set; }
    [Parameter] public Func<TEnum, string>? EnumAriaLabelSelector { get; set; }
    [Parameter] public IEnumerable<TEnum>? ExcludeValues { get; set; }
    [Parameter] public IEnumerable<TEnum>? IncludeOnlyValues { get; set; }
    
    // Ñîáûòèÿ
    [Parameter] public EventCallback<TEnum> OnValueChanged { get; set; }
    [Parameter] public EventCallback<IEnumerable<TEnum>> OnValuesChanged { get; set; }
    [Parameter] public EventCallback<TEnum> OnEnumSelected { get; set; }
    [Parameter] public EventCallback<TEnum> OnEnumDeselected { get; set; }
    [Parameter] public EventCallback<bool> OnBooleanChanged { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _isFlagsEnum;
    private TEnum[] _enumValues = Array.Empty<TEnum>();

    protected override void OnInitialized()
    {
        _isFlagsEnum = typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null;
        _enumValues = GetEnumValues().ToArray();
        
        // Óñòàíàâëèâàåì çàãîëîâîê ãðóïïû ïî óìîë÷àíèþ
        if (ShowAsGroup && string.IsNullOrEmpty(GroupTitle))
        {
            GroupTitle = $"Select {typeof(TEnum).Name}";
        }
    }

    private IEnumerable<TEnum> GetEnumValues()
    {
        // Äëÿ boolean òèïà âîçâðàùàåì ïóñòóþ êîëëåêöèþ
        if (typeof(TEnum) == typeof(bool))
        {
            return Enumerable.Empty<TEnum>();
        }
        
        var values = Enum.GetValues<TEnum>().AsEnumerable();
        
        // Ïðèìåíÿåì ôèëüòðû
        if (IncludeOnlyValues != null)
        {
            values = values.Where(v => IncludeOnlyValues.Contains(v));
        }
        
        if (ExcludeValues != null)
        {
            values = values.Where(v => !ExcludeValues.Contains(v));
        }
        
        return values;
    }

    private bool IsEnumValueChecked(TEnum enumValue)
    {
        if (_isFlagsEnum && SelectedValue.HasValue)
        {
            // Îïðåäåëÿåì òèï enum äëÿ êîððåêòíîé ðàáîòû ñ long çíà÷åíèÿìè
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            var isLongEnum = underlyingType == typeof(long);
            
            if (isLongEnum)
            {
                var selectedLong = Convert.ToInt64(SelectedValue.Value);
                var enumLong = Convert.ToInt64(enumValue);
                return (selectedLong & enumLong) == enumLong && enumLong != 0;
            }
            else
            {
                // Äëÿ flags enum ïðîâåðÿåì áèòîâóþ ìàñêó çíà÷åíèÿ
                var selectedInt = Convert.ToInt32(SelectedValue.Value);
                var enumInt = Convert.ToInt32(enumValue);
                return (selectedInt & enumInt) == enumInt && enumInt != 0;
            }
        }
        
        if (SelectedValues != null)
        {
            return SelectedValues.Contains(enumValue);
        }
        
        return SelectedValue.HasValue && SelectedValue.Value.Equals(enumValue);
    }

    private bool HasAnySelection()
    {
        if (_isFlagsEnum && SelectedValue.HasValue)
        {
            return Convert.ToInt32(SelectedValue.Value) != 0;
        }
        
        return SelectedValues?.Any() == true || SelectedValue.HasValue;
    }

    private string GetEnumText(TEnum enumValue)
    {
        if (EnumTextSelector != null)
            return EnumTextSelector(enumValue);

        // Ïûòàåìñÿ ïîëó÷èòü DisplayAttribute
        var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
        var displayAttribute = memberInfo?.GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute?.Name != null)
            return displayAttribute.Name;

        // Ïûòàåìñÿ ïîëó÷èòü DescriptionAttribute
        var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttribute?.Description != null)
            return descriptionAttribute.Description;

        return enumValue.ToString();
    }

    private string GetEnumIcon(TEnum enumValue)
    {
        if (EnumIconSelector != null)
            return EnumIconSelector(enumValue);
        
        return "";
    }

    private bool HasEnumIcon(TEnum enumValue)
    {
        return !string.IsNullOrWhiteSpace(GetEnumIcon(enumValue));
    }

    private string GetEnumAriaLabel(TEnum enumValue)
    {
        if (EnumAriaLabelSelector != null)
            return EnumAriaLabelSelector(enumValue);
        
        var text = GetEnumText(enumValue);
        var isChecked = IsEnumValueChecked(enumValue);
        
        return $"{text}{(isChecked ? " checked" : " unchecked")}";
    }

    private async Task HandleEnumChange(TEnum enumValue, bool isChecked)
    {
        if (Disabled)
            return;

        if (_isFlagsEnum)
        {
            await HandleFlagsEnumChange(enumValue, isChecked);
        }
        else
        {
            await HandleRegularEnumChange(enumValue, isChecked);
        }
    }

    private async Task HandleFlagsEnumChange(TEnum enumValue, bool isChecked)
    {
        // Îïðåäåëÿåì òèï enum (int, long, etc.)
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        var isLongEnum = underlyingType == typeof(long);
        
        if (isLongEnum)
        {
            var currentValue = SelectedValue.HasValue ? Convert.ToInt64(SelectedValue.Value) : 0L;
            var enumValueLong = Convert.ToInt64(enumValue);

            if (isChecked)
            {
                currentValue |= enumValueLong;
                await OnEnumSelected.InvokeAsync(enumValue);
            }
            else
            {
                currentValue &= ~enumValueLong;
                await OnEnumDeselected.InvokeAsync(enumValue);
            }

            var newValue = (TEnum)Enum.ToObject(typeof(TEnum), currentValue);
            await OnValueChanged.InvokeAsync(newValue);
            
            // Òàêæå âûçûâàåì OnValuesChanged äëÿ ñîâìåñòèìîñòè
            var selectedFlags = GetSelectedFlags(newValue);
            await OnValuesChanged.InvokeAsync(selectedFlags);
        }
        else
        {
            var currentValue = SelectedValue.HasValue ? Convert.ToInt32(SelectedValue.Value) : 0;
            var enumValueInt = Convert.ToInt32(enumValue);

            if (isChecked)
            {
                currentValue |= enumValueInt;
                await OnEnumSelected.InvokeAsync(enumValue);
            }
            else
            {
                currentValue &= ~enumValueInt;
                await OnEnumDeselected.InvokeAsync(enumValue);
            }

            var newValue = (TEnum)Enum.ToObject(typeof(TEnum), currentValue);
            await OnValueChanged.InvokeAsync(newValue);
            
            // Òàêæå âûçûâàåì OnValuesChanged äëÿ ñîâìåñòèìîñòè
            var selectedFlags = GetSelectedFlags(newValue);
            await OnValuesChanged.InvokeAsync(selectedFlags);
        }
    }

    private async Task HandleRegularEnumChange(TEnum enumValue, bool isChecked)
    {
        var currentSelected = SelectedValues?.ToList() ?? new List<TEnum>();

        if (isChecked)
        {
            if (!currentSelected.Contains(enumValue))
            {
                currentSelected.Add(enumValue);
                await OnEnumSelected.InvokeAsync(enumValue);
            }
        }
        else
        {
            if (currentSelected.Contains(enumValue))
            {
                currentSelected.Remove(enumValue);
                await OnEnumDeselected.InvokeAsync(enumValue);
            }
        }

        await OnValuesChanged.InvokeAsync(currentSelected);
        
        // Äëÿ îäèíî÷íîãî âûáîðà òàêæå óñòàíàâëèâàåì SelectedValue
        if (currentSelected.Count == 1)
        {
            await OnValueChanged.InvokeAsync(currentSelected.First());
        }
        else if (currentSelected.Count == 0)
        {
            await OnValueChanged.InvokeAsync(default(TEnum));
        }
    }

    private IEnumerable<TEnum> GetSelectedFlags(TEnum flagsValue)
    {
        var result = new List<TEnum>();
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        var isLongEnum = underlyingType == typeof(long);
        
        if (isLongEnum)
        {
            var flagsLong = Convert.ToInt64(flagsValue);
            
            foreach (var enumValue in _enumValues)
            {
                var enumLong = Convert.ToInt64(enumValue);
                if (enumLong != 0 && (flagsLong & enumLong) == enumLong)
                {
                    result.Add(enumValue);
                }
            }
        }
        else
        {
            var flagsInt = Convert.ToInt32(flagsValue);
            
            foreach (var enumValue in _enumValues)
            {
                var enumInt = Convert.ToInt32(enumValue);
                if (enumInt != 0 && (flagsInt & enumInt) == enumInt)
                {
                    result.Add(enumValue);
                }
            }
        }
        
        return result;
    }

    // Ìåòîäû äëÿ ðàáîòû ñ boolean (åñëè TEnum == bool)
    private bool GetBooleanValue()
    {
        if (typeof(TEnum) == typeof(bool) && SelectedValue.HasValue)
        {
            return Convert.ToBoolean(SelectedValue.Value);
        }
        return false;
    }

    private async Task HandleBooleanChange(bool isChecked)
    {
        if (typeof(TEnum) == typeof(bool))
        {
            var boolValue = (TEnum)(object)isChecked;
            
            await OnValueChanged.InvokeAsync(boolValue);
            await OnBooleanChanged.InvokeAsync(isChecked);
        }
    }

    // Ïóáëè÷íûå ìåòîäû äëÿ óïðàâëåíèÿ ñîñòîÿíèåì
    public void SelectValue(TEnum value)
    {
        if (_isFlagsEnum && SelectedValue.HasValue)
        {
            var currentValue = Convert.ToInt32(SelectedValue.Value);
            var enumValueInt = Convert.ToInt32(value);
            currentValue |= enumValueInt;
            var newValue = (TEnum)Enum.ToObject(typeof(TEnum), currentValue);
            _ = OnValueChanged.InvokeAsync(newValue);
        }
        else
        {
            var currentSelected = SelectedValues?.ToList() ?? new List<TEnum>();
            if (!currentSelected.Contains(value))
            {
                currentSelected.Add(value);
                _ = OnValuesChanged.InvokeAsync(currentSelected);
            }
        }
    }

    public void DeselectValue(TEnum value)
    {
        if (_isFlagsEnum && SelectedValue.HasValue)
        {
            var currentValue = Convert.ToInt32(SelectedValue.Value);
            var enumValueInt = Convert.ToInt32(value);
            currentValue &= ~enumValueInt;
            var newValue = (TEnum)Enum.ToObject(typeof(TEnum), currentValue);
            _ = OnValueChanged.InvokeAsync(newValue);
        }
        else
        {
            var currentSelected = SelectedValues?.ToList() ?? new List<TEnum>();
            if (currentSelected.Contains(value))
            {
                currentSelected.Remove(value);
                _ = OnValuesChanged.InvokeAsync(currentSelected);
            }
        }
    }

    public void ClearSelection()
    {
        if (_isFlagsEnum)
        {
            _ = OnValueChanged.InvokeAsync(default(TEnum));
        }
        else
        {
            _ = OnValuesChanged.InvokeAsync(new List<TEnum>());
        }
    }

    public void ToggleValue(TEnum value)
    {
        if (IsEnumValueChecked(value))
        {
            DeselectValue(value);
        }
        else
        {
            SelectValue(value);
        }
    }

    private string GetCheckboxClass(TEnum enumValue, bool isChecked)
    {
        var classes = new List<string>();
        
        if (isChecked)
            classes.Add("checked");
        
        if (Disabled)
            classes.Add("disabled");
            
        // Ìîæíî äîáàâèòü êàñòîìíûå êëàññû ÷åðåç ñåëåêòîðû â áóäóùåì
        return string.Join(" ", classes);
    }
}