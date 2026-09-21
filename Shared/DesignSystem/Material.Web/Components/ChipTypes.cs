using System.Linq;

namespace Material.Web.Components;

public enum ChipType
{
    Assist,
    Filter,
    Input,
    Suggestion
}

// Âñïîìîãàòåëüíûé êëàññ äëÿ êîíôèãóðàöèè
public class ChipConfiguration
{
    public ChipType ChipType { get; set; } = ChipType.Filter;
    public bool ShowAsChipSet { get; set; } = true;
    public IEnumerable<object>? Items { get; set; }
    public object? SingleValue { get; set; }
    public object? SelectedValue { get; set; }
    public IEnumerable<object>? SelectedValues { get; set; }
    public bool MultiSelect { get; set; } = false;
    public Func<object, string>? ItemTextSelector { get; set; }
    public Func<object, string>? ItemIconSelector { get; set; }
    public Func<object, string>? ItemHrefSelector { get; set; }
    public Func<object, string>? ItemTargetSelector { get; set; }
    public Func<object, string>? ItemClassSelector { get; set; }
    public string? SingleText { get; set; }
    public string? SingleIcon { get; set; }
    public string? SingleHref { get; set; }
    public string? SingleTarget { get; set; }
    public bool Disabled { get; set; } = false;
    public bool Elevated { get; set; } = false;
    public bool Removable { get; set; } = false;
    public bool RemoveOnly { get; set; } = false;
    public bool Avatar { get; set; } = false;
    public string? AriaLabel { get; set; }
    public string? ContainerClass { get; set; }

    // Ñòàòè÷åñêèå ìåòîäû äëÿ enum
    public static ChipConfiguration CreateEnumChip<TEnum>(TEnum? selectedValue = null, bool multiSelect = false, ChipType chipType = ChipType.Filter) 
        where TEnum : struct, Enum
    {
        var enumValues = Enum.GetValues<TEnum>().Cast<object>();
        
        return new ChipConfiguration
        {
            ChipType = chipType,
            Items = enumValues,
            MultiSelect = multiSelect,
            SelectedValue = selectedValue,
            ItemTextSelector = item => item.ToString() ?? "",
            ShowAsChipSet = true
        };
    }

    public static ChipConfiguration CreateFlagsEnumChip<TEnum>(TEnum? selectedValue = null, ChipType chipType = ChipType.Filter) 
        where TEnum : struct, Enum
    {
        var enumValues = Enum.GetValues<TEnum>().Cast<object>();
        var selectedValues = new List<object>();
        
        if (selectedValue.HasValue)
        {
            foreach (TEnum enumValue in Enum.GetValues<TEnum>())
            {
                if (selectedValue.Value.HasFlag(enumValue))
                {
                    selectedValues.Add(enumValue);
                }
            }
        }
        
        return new ChipConfiguration
        {
            ChipType = chipType,
            Items = enumValues,
            MultiSelect = true,
            SelectedValues = selectedValues,
            ItemTextSelector = item => item.ToString() ?? "",
            ShowAsChipSet = true
        };
    }
}