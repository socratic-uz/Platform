using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Material.Web.Components;

public partial class Radio : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public bool? Checked { get; set; }
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public EventCallback OnChange { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected bool ComputedChecked => Checked.HasValue 
        ? Checked.Value 
        : (!string.IsNullOrEmpty(Value) && !string.IsNullOrEmpty(OptionValue) && Value == OptionValue);

    protected async Task HandleChange(ChangeEventArgs e)
    {
        var isChecked = e.Value is bool b ? b : (e.Value?.ToString() == "true" || e.Value?.ToString() == "on");
        if (Checked.HasValue)
        {
            await CheckedChanged.InvokeAsync(isChecked);
        }

        var val = OptionValue ?? Value ?? e.Value?.ToString() ?? "";
        await ValueChanged.InvokeAsync(val);

        if (OnChange.HasDelegate)
        {
            await OnChange.InvokeAsync();
        }
    }
}
