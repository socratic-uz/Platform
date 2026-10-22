using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Material.Web.Components;

public partial class Slider : InputBase<decimal>
{
    protected ElementReference sliderElement;

    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public decimal Min { get; set; } = 0;
    [Parameter] public decimal Max { get; set; } = 100;
    [Parameter] public decimal Step { get; set; } = 1;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool ShowTicks { get; set; } = false;
    [Parameter] public bool ShowLabel { get; set; } = false;
    [Parameter] public string ValueLabel { get; set; } = "";
    [Parameter] public string AriaLabel { get; set; } = "";
    [Parameter] public string AriaValueText { get; set; } = "";

    [Parameter] public Expression<Func<decimal>>? For { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    [Parameter] public EventCallback<ChangeEventArgs> OnSlide { get; set; }

    protected bool HasValidationErrors => EditContext?.GetValidationMessages(FieldIdentifier).Any() == true;

    protected async Task HandleInput(ChangeEventArgs e)
    {
        var parsed = ParseDecimal(e.Value);
        if (parsed.HasValue)
        {
            CurrentValue = parsed.Value;
            await OnSlide.InvokeAsync(e);
        }
    }

    protected async Task HandleChange(ChangeEventArgs e)
    {
        var parsed = ParseDecimal(e.Value);
        if (parsed.HasValue)
        {
            CurrentValue = parsed.Value;
        }
        await Task.CompletedTask;
    }

    private decimal? ParseDecimal(object? val)
    {
        if (val is null) return null;
        if (val is decimal d) return d;
        if (val is double db) return (decimal)db;
        if (val is int i) return i;
        if (val is long l) return l;
        if (val is float f) return (decimal)f;
        
        var str = val.ToString();
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var res1))
            return res1;
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out var res2))
            return res2;
            
        return null;
    }

    protected override string? FormatValueAsString(decimal value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    protected override void OnParametersSet()
    {
        if (For != null && EditContext != null)
        {
            FieldIdentifier = FieldIdentifier.Create(For);
        }
        base.OnParametersSet();
    }

    protected override bool TryParseValueFromString(string? value, out decimal result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = 0;
            validationErrorMessage = null;
            return true;
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            if (result < Min || result > Max)
            {
                validationErrorMessage = $"Значение должно быть между {Min} и {Max}";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }

        validationErrorMessage = "Неверный числовой формат";
        return false;
    }
}
