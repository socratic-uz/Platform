using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

using SharedKernel.ValueObjects;

using Shared.Extensions;

namespace Shared.Services
{
    /// <summary>
    /// Represents a service that provides localized strings.
    /// </summary>
    public class StringLocalizer : IStringLocalizer
    {
        private IStringLocalizer _localizer;
        /// <summary>
        /// Creates a new <see cref="StringLocalizer"/>.
        /// </summary>
        /// <param name="factory">The <see cref="IStringLocalizerFactory"/> to use.</param>
        public StringLocalizer(IStringLocalizerFactory factory, ISettingsManager settings)
        {
            //ArgumentNullThrowHelper.ThrowIfNull(factory);
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));


            Task.Run(async () => await settings.GetLanguageAsync())
                .ContinueWith(async task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"Ошибка: {task.Exception}");
                    }
                    else
                    {
                        var language = await task;
                        _localizer = factory.Create(language.ToResourceType());
                    }
                });

            _localizer = factory.Create(Language.Uz.ToResourceType());
        }

        /// <inheritdoc />
        public virtual LocalizedString this[string name]
        {
            get
            {
                //ArgumentNullThrowHelper.ThrowIfNull(name);
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                return _localizer[name];
            }
        }

        /// <inheritdoc />
        public virtual LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                //ArgumentNullThrowHelper.ThrowIfNull(name);
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                return _localizer[name, arguments];
            }
        }

        /// <inheritdoc />
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _localizer.GetAllStrings(includeParentCultures);
    }
}