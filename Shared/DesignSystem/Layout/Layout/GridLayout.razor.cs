using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace Shared.Layout
{
    public partial class GridLayout : LayoutComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public ErrorBoundary? ErrorBoundaryRef { get; set; }

        public void Recover() => ErrorBoundaryRef?.Recover();
    }
}
