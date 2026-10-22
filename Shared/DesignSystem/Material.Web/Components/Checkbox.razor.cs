using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Material.Web.Components;

public partial class Checkbox : InputBase<bool>
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Required { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Indeterminate { get; set; } = false;
    [Parameter] public string FormValue { get; set; } = "on";
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<ChangeEventArgs> OnChangeCallback { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs> OnInputCallback { get; set; }

    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (bool.TryParse(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }

        result = !string.IsNullOrEmpty(value) && (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == FormValue);
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
        await OnChangeCallback.InvokeAsync(e);
    }

    protected async Task HandleInput(ChangeEventArgs e)
    {
        UpdateCurrentValueFromEvent(e);
        await OnInputCallback.InvokeAsync(e);
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
}
