using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Microsoft.Extensions.Localization;
using Assets.Constants;
using static Assets.Constants.Classes.Picker;
using static Assets.Constants.Classes.ColorPicker;
using static Assets.Constants.Formats;

namespace Material.Web.Components;

public partial class ColorPicker : ComponentBase
{
    protected string CompactBoxClass => Disabled
        ? (string.IsNullOrEmpty(CssClass) ? string.Format(Two, CompactBox, Classes.Picker.Disabled) : string.Format(Three, CompactBox, CssClass, Classes.Picker.Disabled))
        : (string.IsNullOrEmpty(CssClass) ? CompactBox : string.Format(Two, CompactBox, CssClass));

    protected string GetPaletteSwatchClass(bool isSelected) => isSelected
        ? string.Format(Two, PaletteSwatch, Classes.Picker.Selected)
        : PaletteSwatch;

    protected string HexCompactClass => string.Format(Two, HexWrapper, HexCompact);
    [Inject] public IStringLocalizer L { get; set; } = default!;

    [Parameter] public string? Value { get; set; } = "#6750A4";
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public EventCallback OnReset { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Compact { get; set; } = false;
    [Parameter] public bool ShowPresets { get; set; } = true;
    [Parameter] public string[]? CustomPresets { get; set; }

    [Inject] protected IJSRuntime JS { get; set; } = null!;

    protected bool isPaletteOpen = false;
    protected bool isCopied = false;

    protected bool HasValue => !string.IsNullOrWhiteSpace(Value);

    protected string EffectiveLabel => Label ?? (L != null ? L[Loc.Common.ColorPickerColorSelection] : "Color selection");

    protected string GetPreviewText() => HasValue 
        ? string.Format(L != null ? L[Loc.Common.ColorPickerColorSelected] : "Selected: {0}", NormalizedValue) 
        : (L != null ? L[Loc.Common.ColorPickerColorDefault] : "Color not selected");

    public static Dictionary<string, object> GetSwatchAttributes(string color) => new() { ["style"] = $"background-color: {color};" };

    private static readonly string[] DefaultPresets = new[]
    {
        "#6750A4", // MD3 Primary Purple
        "#00639B", // Ocean Blue
        "#006874", // Cyan / Teal
        "#006D44", // Emerald Green
        "#7E525E", // Dusty Rose
        "#8B5000", // Warm Amber
        "#BA1A1A", // Material Red / Error
        "#1E293B", // Dark Slate
        "#475569", // Medium Slate
        "#64748B", // Cool Gray
        "#F8FAFC", // Pure Light
        "#FFFFFF"  // White
    };

    protected string[] EffectivePresets => CustomPresets is { Length: > 0 } ? CustomPresets : DefaultPresets;

    protected string NormalizedValue => string.IsNullOrWhiteSpace(Value) ? "#6750A4" : (Value.StartsWith("#") ? Value : $"#{Value}");

    protected string HexValue
    {
        get
        {
            var val = NormalizedValue;
            return val.Length == 7 ? val : (val.Length > 7 ? val.Substring(0, 7) : "#6750A4");
        }
    }

    protected string RawHexValue => NormalizedValue.TrimStart('#');

    protected async Task OnNativeColorChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString() ?? "#000000";
        await SetValueAsync(val);
    }

    protected async Task OnNativeColorInput(ChangeEventArgs e)
    {
        var val = e.Value?.ToString() ?? "#000000";
        await SetValueAsync(val);
    }

    protected async Task OnTextInputChanged(ChangeEventArgs e)
    {
        var text = e.Value?.ToString()?.Trim() ?? "";
        if (!text.StartsWith("#")) text = $"#{text}";
        if (Regex.IsMatch(text, @"^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$"))
        {
            await SetValueAsync(text);
        }
    }

    protected async Task SelectPreset(string color)
    {
        await SetValueAsync(color);
        isPaletteOpen = false;
    }

    protected async Task SetValueAsync(string? val)
    {
        Value = val;
        await ValueChanged.InvokeAsync(val);
    }

    protected void TogglePalette()
    {
        isPaletteOpen = !isPaletteOpen;
    }

    protected void ClosePalette()
    {
        isPaletteOpen = false;
    }

    protected async Task CopyColorAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", NormalizedValue);
            isCopied = true;
            StateHasChanged();
            await Task.Delay(1500);
            isCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Clipboard fallback
        }
    }

    public async Task Clear()
    {
        await SetValueAsync(null);
        await OnReset.InvokeAsync();
    }
}
