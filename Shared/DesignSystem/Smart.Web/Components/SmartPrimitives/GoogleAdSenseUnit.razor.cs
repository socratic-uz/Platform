using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Smart.Web.Components
{
    public partial class GoogleAdSenseUnit : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;

        /// <summary>
        /// Google AdSense Publisher ID (например ca-pub-3226044876387611)
        /// </summary>
        [Parameter] public string Client { get; set; } = "ca-pub-3226044876387611";

        /// <summary>
        /// Идентификатор рекламного блока (slot id)
        /// </summary>
        [Parameter] public string? Slot { get; set; }

        /// <summary>
        /// Формат объявления (auto, horizontal, rectangle, etc.)
        /// </summary>
        [Parameter] public string Format { get; set; } = "auto";

        /// <summary>
        /// Адаптивная ширина
        /// </summary>
        [Parameter] public bool FullWidthResponsive { get; set; } = true;

        [Parameter] public string? Style { get; set; }
        [Parameter] public string? InsStyle { get; set; } = "display:block;";
        [Parameter] public string? Class { get; set; }
        [Parameter] public bool Visible { get; set; } = true;

        private bool _isInitialized;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Visible && !_isInitialized)
            {
                _isInitialized = true;
                await PushAdAsync();
            }
        }

        private async ValueTask PushAdAsync()
        {
            try
            {
                await JS.InvokeVoidAsync("socraticAdSense.push");
            }
            catch
            {
                // Защита от блокировщиков рекламы (AdBlock), отключенного JS или статичного SSR
            }
        }
    }
}
