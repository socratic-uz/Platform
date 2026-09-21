using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;





namespace Material.Web.Components;

public partial class ChipSet
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public ChipType ChipType { get; set; } = ChipType.Filter;
    [Parameter] public Enum? EnumData { get; set; }
    [Parameter] public EventCallback<Enum?> EnumDataChanged { get; set; }
    [Parameter] public Expression<Func<Enum?>> EnumDataExpression { get; set; } = default!;

    [Parameter] public IEnumerable<object>? Data { get; set; }
    [Parameter] public EventCallback<IEnumerable<object>?> DataChanged { get; set; }
    [Parameter] public object? Selected { get; set; }
    [Parameter] public EventCallback<object?> SelectedChanged { get; set; }



    bool isFlag;

    protected override Task OnParametersSetAsync()
    {
        isFlag = EnumData?.GetType().GetCustomAttribute<FlagsAttribute>() != null;
        return Task.CompletedTask;
    }

    async Task OnFlagClick(object item)
    {
        if (EnumData == null) return;
        var enumType = EnumData.GetType();
        var enumValue = Convert.ToInt64(EnumData);
        var itemValue = Convert.ToInt64(item);
        if (EnumData.HasFlag((Enum)item))
            EnumData = (Enum)Enum.ToObject(enumType, enumValue & ~itemValue);
        else
            EnumData = (Enum)Enum.ToObject(enumType, enumValue | itemValue);
        await EnumDataChanged.InvokeAsync(EnumData);
        StateHasChanged();
    }
    async Task OnEnumClick(object item)
    {
        EnumData = (Enum)item;
        await EnumDataChanged.InvokeAsync(EnumData);
        StateHasChanged();
    }
    async Task OnObjClick(object item)
    {
        await SelectedChanged.InvokeAsync(item); 
        StateHasChanged();
    }




}