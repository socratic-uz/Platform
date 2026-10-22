using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Services;

namespace Shared.Layout
{
    public partial class Color : ComponentBase
    {
        [Inject] public IJSRuntime Js { get; set; } = default!;
        [Inject] public ISettingsManager Settings { get; set; } = default!;

        [Parameter] public SharedKernel.ValueObjects.Theme? Theme { get; set; }
        [Parameter] public int? Accent { get; set; }

        private bool _isDark;

        public string ModeClass => _isDark ? "dark-mode" : "light-mode";

        public static string ToHex(int val)
        {
            if (val <= 3)
            {
                return val switch
                {
                    0 => "#00ffb3",
                    1 => "#3b82f6",
                    2 => "#ef4444",
                    3 => "#eab308",
                    _ => "#00ffb3"
                };
            }
            var rgb = val & 0xFFFFFF;
            return $"#{rgb:x6}";
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Accent ??= await Settings.GetAccentAsync();
                Theme ??= await Settings.GetThemeAsync();

                if (Theme == SharedKernel.ValueObjects.Theme.Dark || Theme == null)
                {
                    _isDark = true;
                }
                else if (Theme == SharedKernel.ValueObjects.Theme.Light)
                {
                    _isDark = false;
                }
            }
            catch
            {
                _isDark = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    Accent ??= await Settings.GetAccentAsync();
                    Theme ??= await Settings.GetThemeAsync();

                    var hexAccent = ToHex(Accent.Value);
                    var module = await Js.InvokeAsync<IJSObjectReference>("import", "./_content/Layout/Layout/Color.razor.js");

                    if (Theme == SharedKernel.ValueObjects.Theme.System)
                    {
                        _isDark = await module.InvokeAsync<bool>("isSystemDark");
                    }
                    else
                    {
                        _isDark = Theme == SharedKernel.ValueObjects.Theme.Dark;
                    }

                    await module.InvokeAsync<string>("applyTheme", hexAccent, _isDark);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Color.razor error: {e.Message}");
                }
            }
        }
    }
}
