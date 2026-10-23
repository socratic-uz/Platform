using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SharedKernel.ValueObjects;
using Shared.Services;

namespace Shared.Layout
{
    public partial class LanguageSelector : ComponentBase
    {
        [Inject] public ISettingsManager Settings { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IStringLocalizer L { get; set; } = default!;

        [Parameter] public string Variant { get; set; } = "pill";
        [Parameter] public string Class { get; set; } = string.Empty;

        public const string LangSelectorInlineClass = "lang-selector-inline";
        public const string LangSelectorPillClass = "lang-selector-pill";
        public const string LangMenuBackdropClass = "lang-menu-backdrop";
        public const string LangMenuItemActiveClass = "lang-menu-item--active";
        public const string LangFlagClass = "lang-flag";
        public const string LangNameClass = "lang-name";

        public Language CurrentLanguage { get; set; } = Language.Ru;
        public bool IsOpen { get; set; }

        public bool IsUz => CurrentLanguage == Language.Uz;
        public bool IsRu => CurrentLanguage == Language.Ru;
        public bool IsEn => CurrentLanguage == Language.En;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                CurrentLanguage = await Settings.GetLanguageAsync();
            }
            catch
            {
                CurrentLanguage = Language.Ru;
            }
        }

        public void ToggleDropdown() => IsOpen = !IsOpen;
        public void CloseDropdown() => IsOpen = false;

        public Task SelectUz() => SelectLanguage(Language.Uz);
        public Task SelectRu() => SelectLanguage(Language.Ru);
        public Task SelectEn() => SelectLanguage(Language.En);

        public async Task SelectLanguage(Language lang)
        {
            IsOpen = false;
            if (CurrentLanguage == lang) return;
            CurrentLanguage = lang;
            try
            {
                await Settings.SetLanguageAsync(lang);
            }
            catch { }
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        }

        public static string GetLangShortLabel(Language lang) => lang switch
        {
            Language.Uz => "UZ",
            Language.Ru => "RU",
            Language.En => "EN",
            _ => "RU"
        };
    }
}
