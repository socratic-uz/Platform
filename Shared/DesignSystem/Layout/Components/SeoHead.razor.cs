using System;
using Microsoft.AspNetCore.Components;

namespace Shared.Layout
{
    public partial class SeoHead : ComponentBase
    {
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public string? Keywords { get; set; }
        [Parameter] public string? CanonicalUrl { get; set; }
        [Parameter] public string? ImageUrl { get; set; }
        [Parameter] public string? Type { get; set; }
        [Parameter] public string? JsonLd { get; set; }
        [Parameter] public bool NoIndex { get; set; }

        public const string DefaultTitle = "Socratic - Универсальная AI-платформа для бизнеса, торговли и сферы услуг";
        public const string DefaultImage = "https://socratic.uz/img/icon-512.png";

        public string GetLocalizedUrl(string culture)
        {
            if (string.IsNullOrWhiteSpace(CanonicalUrl)) return string.Empty;
            var uri = new Uri(CanonicalUrl, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri) return CanonicalUrl;

            var builder = new UriBuilder(uri);
            var query = System.Web.HttpUtility.ParseQueryString(builder.Query);
            query["culture"] = culture;
            builder.Query = query.ToString();
            return builder.ToString();
        }
    }
}
