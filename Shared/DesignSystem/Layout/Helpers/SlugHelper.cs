using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class SlugHelper
    {
        private static readonly Dictionary<string, string> TranslitMap = new(StringComparer.OrdinalIgnoreCase)
        {
            {"а", "a"}, {"б", "b"}, {"в", "v"}, {"г", "g"}, {"д", "d"},
            {"е", "e"}, {"ё", "yo"}, {"ж", "zh"}, {"з", "z"}, {"и", "i"},
            {"й", "y"}, {"к", "k"}, {"л", "l"}, {"м", "m"}, {"н", "n"},
            {"о", "o"}, {"п", "p"}, {"р", "r"}, {"с", "s"}, {"т", "t"},
            {"у", "u"}, {"ф", "f"}, {"х", "kh"}, {"ц", "ts"}, {"ч", "ch"},
            {"ш", "sh"}, {"щ", "shch"}, {"ъ", ""}, {"ы", "y"}, {"ь", ""},
            {"э", "e"}, {"ю", "yu"}, {"я", "ya"}, {"ў", "o"}, {"қ", "q"},
            {"ғ", "g"}, {"ҳ", "h"}
        };

        public static string GenerateSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "item";

            var lower = text.Trim().ToLowerInvariant();
            var sb = new StringBuilder();

            for (int i = 0; i < lower.Length; i++)
            {
                var chStr = lower[i].ToString();
                if (TranslitMap.TryGetValue(chStr, out var translit))
                {
                    sb.Append(translit);
                }
                else
                {
                    sb.Append(lower[i]);
                }
            }

            var transliterated = sb.ToString();
            sb.Clear();
            bool prevDash = false;

            foreach (char c in transliterated)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevDash = false;
                }
                else if (c == ' ' || c == '-' || c == '_' || c == '/' || c == '.')
                {
                    if (!prevDash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevDash = true;
                    }
                }
            }

            var result = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? "item" : result;
        }
    }
}
