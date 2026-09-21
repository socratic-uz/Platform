using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

using SharedKernel.ValueObjects;

using Shared.Resources;
using Shared.Services;

namespace Shared.Extensions
{
    public static class LocalizationExtension
    {
        public static IStringLocalizer Create(this IStringLocalizerFactory factory, Language language)
            => factory.Create(language.ToResourceType());

        public static async Task<IStringLocalizer> Create(this IStringLocalizerFactory factory, ISettingsManager settings)
            => factory.Create(await settings.GetLanguageAsync());

        public static Type ToResourceType(this Language language)
        {
            return language switch
            {
                Language.Uz => typeof(ResourceUz),
                Language.Ru => typeof(ResourceRu),
                Language.En => typeof(ResourceEn),
                _ => typeof(ResourceUz),
            };
        }
    }
}