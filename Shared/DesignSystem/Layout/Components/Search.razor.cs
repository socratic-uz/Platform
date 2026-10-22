using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class Search : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        [Parameter] public EventCallback<string> OnSubmit { get; set; }

        private string? _searchText;

        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnSubmit.InvokeAsync(value);
            }
        }

        public void ClearSearch()
        {
            SearchText = string.Empty;
        }

        public string T(string key, string fallback)
        {
            if (L == null) return fallback;
            var loc = L[key];
            return (loc == null || loc.ResourceNotFound || string.IsNullOrWhiteSpace(loc.Value) || loc.Value == key) ? fallback : loc.Value;
        }
    }
}
