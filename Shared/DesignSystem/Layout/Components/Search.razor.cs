using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class Search : ComponentBase
    {
        protected const string SearchIcon = global::Assets.Constants.Icons.Search;
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public const string SearchInputFieldClass = "search-input-field";

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
    }
}
