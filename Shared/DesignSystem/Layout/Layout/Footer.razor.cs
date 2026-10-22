using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class Footer : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string Class { get; set; } = string.Empty;

        public string T(string key, string fallback)
        {
            if (L == null) return fallback;
            try
            {
                var localized = L[key];
                if (localized == null || localized.ResourceNotFound || string.IsNullOrWhiteSpace(localized.Value) || localized.Value == key)
                {
                    return fallback;
                }
                return localized.Value;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
