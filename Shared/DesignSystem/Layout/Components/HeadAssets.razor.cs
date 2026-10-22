using Microsoft.AspNetCore.Components;

namespace Shared.Components
{
    public partial class HeadAssets : ComponentBase
    {
        /// <summary>
        /// Базовый префикс для подключения статических ресурсов Assets.
        /// По умолчанию: "_content/Assets/". Для локальных превью файлов: "../../".
        /// </summary>
        [Parameter] public string BasePath { get; set; } = "_content/Assets/";

        /// <summary>
        /// Включать ли тег &lt;base href&gt;. По умолчанию true. Отключается в статическом превью.
        /// </summary>
        [Parameter] public bool IncludeBase { get; set; } = true;

        /// <summary>
        /// Базовый URL приложения для тега base href. По умолчанию "/".
        /// </summary>
        [Parameter] public string BaseHref { get; set; } = "/";

        /// <summary>
        /// Включать ли внешние рекламные сети (Google AdSense, Yandex) и поисковую индексацию.
        /// </summary>
        [Parameter] public bool IncludeMarketingAndAds { get; set; } = true;

        /// <summary>
        /// Включать ли Telegram WebApp SDK скрипт.
        /// </summary>
        [Parameter] public bool IncludeTelegram { get; set; } = true;
    }
}
