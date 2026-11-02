using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Components
{
    public partial class SettingsPanel : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        [Parameter] public string ThemeMode { get; set; } = "system";
        [Parameter] public EventCallback<string> ThemeModeChanged { get; set; }

        [Parameter] public string AccentColor { get; set; } = "#6750A4";
        [Parameter] public EventCallback<string> AccentColorChanged { get; set; }

        public async Task SetThemeAsync(string mode)
        {
            ThemeMode = mode;
            await ThemeModeChanged.InvokeAsync(mode);
        }
    }
}
