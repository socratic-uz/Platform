using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Shared.Layout
{
    public partial class GridLayout : LayoutComponentBase
    {
        public ErrorBoundary? ErrorBoundaryRef { get; set; }

        public void Recover() => ErrorBoundaryRef?.Recover();
    }
}
