using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Material.Web.Components;

namespace Smart.Web.Components;

public partial class SmartEnum<TEnum> : InputBase<TEnum> where TEnum : struct, Enum
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public SmartEnumDisplayType DisplayType { get; set; } = SmartEnumDisplayType.Auto;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Required { get; set; } = false;

    protected static readonly TEnum[] EnumValues = Enum.GetValues<TEnum>();
    protected static readonly bool IsFlagsEnum = typeof(TEnum).GetCustomAttributes(typeof(FlagsAttribute), false).Any();

    protected SmartEnumDisplayType ResolvedDisplayType
    {
        get
        {
            if (IsFlagsEnum)
            {
                if (DisplayType == SmartEnumDisplayType.Chips)
                    return SmartEnumDisplayType.Chips;

                return SmartEnumDisplayType.Checkboxes;
            }
            else
            {
                if (DisplayType == SmartEnumDisplayType.Checkboxes)
                    return SmartEnumDisplayType.Radio;

                if (DisplayType != SmartEnumDisplayType.Auto)
                    return DisplayType;

                return EnumValues.Length <= 5 ? SmartEnumDisplayType.Segmented : SmartEnumDisplayType.Dropdown;
            }
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<TEnum, string> EnumTextCache = new();

    protected string GetEnumText(TEnum val)
    {
        return EnumTextCache.GetOrAdd(val, v =>
        {
            var memberInfo = typeof(TEnum).GetMember(v.ToString() ?? "").FirstOrDefault();
            if (memberInfo != null)
            {
                var display = memberInfo.GetCustomAttribute<DisplayAttribute>();
                if (display?.Name != null) return display.Name;

                var desc = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                if (desc?.Description != null) return desc.Description;
            }
            return v.ToString();
        });
    }

    protected bool IsSelected(TEnum val)
    {
        if (IsFlagsEnum)
        {
            var currentVal = Convert.ToInt64(CurrentValue);
            var targetVal = Convert.ToInt64(val);
            if (targetVal == 0) return currentVal == 0;
            return (currentVal & targetVal) == targetVal;
        }
        return val.Equals(CurrentValue);
    }

    protected async Task HandleSelect(TEnum val)
    {
        if (Disabled) return;
        CurrentValue = val;
        await Task.CompletedTask;
    }

    protected async Task ToggleValue(TEnum val)
    {
        if (Disabled) return;

        if (IsFlagsEnum)
        {
            var currentVal = Convert.ToInt64(CurrentValue);
            var targetVal = Convert.ToInt64(val);
            long newVal;
            if (targetVal == 0)
            {
                newVal = 0;
            }
            else
            {
                newVal = currentVal ^ targetVal;
            }
            CurrentValue = (TEnum)Enum.ToObject(typeof(TEnum), newVal);
        }
        else
        {
            CurrentValue = val;
        }
        await Task.CompletedTask;
    }

    protected async Task HandleCheckboxToggle(TEnum val, bool checkedState)
    {
        if (Disabled) return;

        var currentVal = Convert.ToInt64(CurrentValue);
        var targetVal = Convert.ToInt64(val);
        long newVal;
        if (checkedState)
        {
            newVal = currentVal | targetVal;
        }
        else
        {
            newVal = currentVal & ~targetVal;
        }
        CurrentValue = (TEnum)Enum.ToObject(typeof(TEnum), newVal);
        await Task.CompletedTask;
    }

    protected List<Select.SelectOption> GetSelectOptions()
    {
        var options = new List<Select.SelectOption>();
        foreach (var val in EnumValues)
        {
            options.Add(new Select.SelectOption(val.ToString(), GetEnumText(val)));
        }
        return options;
    }

    protected async Task HandleDropdownChange(string value)
    {
        if (Disabled) return;

        if (Enum.TryParse<TEnum>(value, out var parsed))
        {
            CurrentValue = parsed;
        }
        await Task.CompletedTask;
    }

    protected async Task HandleRadioSelect(TEnum val, string selectedVal)
    {
        if (Disabled) return;
        CurrentValue = val;
        await Task.CompletedTask;
    }

    protected override bool TryParseValueFromString(string? value, out TEnum result, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (Enum.TryParse<TEnum>(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }
        validationErrorMessage = "Invalid enum value";
        return false;
    }
}
