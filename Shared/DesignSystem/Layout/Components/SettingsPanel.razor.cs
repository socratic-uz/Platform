using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Shared.Components
{
    public partial class SettingsPanel : ComponentBase
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public const string SettingsPanelContainerClass = "settings-panel-container";
        public const string SettingsSectionClass = "settings-section";

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
