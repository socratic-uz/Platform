namespace Shared.Extensions
{
    public static class ColorExtension
    {
        /// <summary>
        /// Argb To Hex
        /// </summary>
        /// <param name="argb"></param>
        /// <returns></returns>
        public static string ToHex(this long argb)
        {
            var alpha = (byte)(argb >> 24);
            var red = (byte)(argb >> 16);
            var green = (byte)(argb >> 8);
            var blue = (byte)argb;

            //%02x форматирует вывод в шестнадцатеричном виде, дополняя нулями слева до двух символов
            return $"#{red:X}{green:X}{blue:X}";
        }
    }
}
