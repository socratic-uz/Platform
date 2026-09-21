using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Shared.Components
{
    public partial class SettingsPanel : ComponentBase
    {
        [Parameter] public string ThemeMode { get; set; } = "system";
        [Parameter] public EventCallback<string> ThemeModeChanged { get; set; }

        [Parameter] public string AccentColor { get; set; } = "#6750A4";
        [Parameter] public EventCallback<string> AccentColorChanged { get; set; }

        private async Task SetTheme(string mode)
        {
            ThemeMode = mode;
            await ThemeModeChanged.InvokeAsync(mode);
        }
    }
}
