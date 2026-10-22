using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Material.Web.Components;

public partial class Switch : InputBase<bool>
{
    [Parameter] public bool Icons { get; set; } = false;
    [Parameter] public bool ShowOnlySelectedIcon { get; set; } = false;
    [Parameter] public bool Required { get; set; } = false;
    [Parameter] public string FormValue { get; set; } = "on";
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs> OnChange { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }

    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = !string.IsNullOrEmpty(value) && bool.TryParse(value, out var parsed) && parsed;
        validationErrorMessage = null;
        return true;
    }

    protected override string? FormatValueAsString(bool value)
    {
        return value.ToString().ToLowerInvariant();
    }

    protected async Task HandleChange(ChangeEventArgs e)
    {
        UpdateCurrentValueFromEvent(e);
        await OnChange.InvokeAsync(e);
    }

    protected async Task HandleInput(ChangeEventArgs e)
    {
        UpdateCurrentValueFromEvent(e);
        await OnInput.InvokeAsync(e);
    }

    private void UpdateCurrentValueFromEvent(ChangeEventArgs e)
    {
        if (e.Value is bool b)
        {
            CurrentValue = b;
        }
        else if (bool.TryParse(e.Value?.ToString(), out var boolVal))
        {
            CurrentValue = boolVal;
        }
        else
        {
            var strVal = e.Value?.ToString();
            CurrentValue = strVal != null && (strVal.Equals("true", StringComparison.OrdinalIgnoreCase) || strVal == FormValue);
        }
    }

    public bool IsSelected => CurrentValue;
}
