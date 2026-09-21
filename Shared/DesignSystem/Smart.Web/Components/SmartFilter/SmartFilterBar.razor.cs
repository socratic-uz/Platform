using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Smart.Web.Components;

public class FilterChip
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public object? Value { get; set; }
}

public partial class SmartFilterBar<TItem> : ComponentBase where TItem : class
{
    [Parameter] public string Placeholder { get; set; } = "Поиск...";
    [Parameter] public string SearchText { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> SearchTextChanged { get; set; }
    [Parameter] public List<FilterChip>? FilterChips { get; set; }
    [Parameter] public EventCallback<FilterChip> OnChipToggled { get; set; }

    private async Task OnSearchValueChanged(string val)
    {
        SearchText = val ?? string.Empty;
        await SearchTextChanged.InvokeAsync(SearchText);
    }

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        await SearchTextChanged.InvokeAsync(SearchText);
    }

    private async Task ClearSearch()
    {
        SearchText = string.Empty;
        await SearchTextChanged.InvokeAsync(SearchText);
    }

    private async Task ToggleChip(FilterChip chip)
    {
        chip.IsSelected = !chip.IsSelected;
        await OnChipToggled.InvokeAsync(chip);
    }
}
