using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Assets.Constants;

namespace Material.Web.Components;

public partial class PickerTriggerCard : ComponentBase
{
    [Parameter] public string Label { get; set; } = "Выбор";
    [Parameter] public string Icon { get; set; } = Icons.Tune;
    [Parameter] public string? ActiveIcon { get; set; }
    [Parameter] public RenderFragment? CustomIconBadge { get; set; }

    [Parameter] public string? BadgeText { get; set; }
    [Parameter] public string? BadgeIcon { get; set; }

    [Parameter] public string? PreviewText { get; set; }
    [Parameter] public RenderFragment? CustomPreview { get; set; }

    [Parameter] public string? ActionLabel { get; set; } = "Выбрать";
    [Parameter] public string? ActionIcon { get; set; } = Icons.Edit;
    [Parameter] public EventCallback<MouseEventArgs> OnAction { get; set; }

    [Parameter] public bool HasValue { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;

    [Parameter] public bool ShowResetButton { get; set; } = true;
    [Parameter] public string ResetIcon { get; set; } = Icons.Close;
    [Parameter] public string ResetAriaLabel { get; set; } = "Сбросить значение";
    [Parameter] public EventCallback OnReset { get; set; }

    [Parameter] public RenderFragment? CustomActions { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    protected string CardClasses => $"{Classes.Picker.TriggerCard} {(Disabled ? Classes.Picker.Disabled : "")} {(HasValue ? "has-value" : "empty")} {Class}".Trim();
    protected string BadgeClasses => HasValue ? $"{Classes.Picker.IconBadge} {Classes.Picker.Active}" : Classes.Picker.IconBadge;

    protected async Task HandleAction(MouseEventArgs e)
    {
        if (Disabled) return;
        await OnAction.InvokeAsync(e);
    }

    protected async Task HandleReset()
    {
        if (Disabled) return;
        await OnReset.InvokeAsync();
    }
}
