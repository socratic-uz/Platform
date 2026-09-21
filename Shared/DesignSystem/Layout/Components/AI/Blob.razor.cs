using System.Globalization;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Shared.Components
{
    public partial class Blob : ComponentBase
    {
        string?[] d = new string[5], f = new string[5];
        protected override Task OnInitializedAsync()
        {
            d[0] ??= GenerateBlobPathDataAnimationValues(15, 100, 15, 50);
            d[1] ??= GenerateBlobPathDataAnimationValues(15, 50, 15, 30);
            d[2] ??= GenerateBlobPathDataAnimationValues(15, 50, 15, 30);
            d[3] ??= GenerateBlobPathDataAnimationValues(15, 200, 15, 100);
            d[4] ??= GenerateBlobPathDataAnimationValues(15, 200, 15, 100);

            f[0] ??= GenerateBlobPathFillAnimationValues(5);
            f[1] ??= GenerateBlobPathFillAnimationValues(5);
            f[2] ??= GenerateBlobPathFillAnimationValues(5);
            f[3] ??= GenerateBlobPathFillAnimationValues(5);
            f[4] ??= GenerateBlobPathFillAnimationValues(5);

            return Task.CompletedTask;
        }

        public string GenerateBlobPathData(int radius, int pointsCount, int variation, Random? rnd = null)
        {
            rnd ??= new Random();
            float[] xs = new float[pointsCount];
            float[] ys = new float[pointsCount];

            // Генерация точек на окружности с вариацией радиуса
            for (int i = 0; i < pointsCount; i++)
            {
                double angle = 2 * Math.PI * i / pointsCount;
                double r = radius + rnd.Next(-variation, variation);

                xs[i] = (float)(Math.Cos(angle) * r);
                ys[i] = (float)(Math.Sin(angle) * r);
            }

            StringBuilder sb = new StringBuilder();

            // Начинаем с первой точки
            //sb.AppendFormat(CultureInfo.InvariantCulture, "M {0},{1} ", xs[0], ys[0]);
            //Начинаем с середины между последней и первой точкой для плавности
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "M{0},{1}",
                (xs[pointsCount - 1] + xs[0]) / 2f,
                (ys[pointsCount - 1] + ys[0]) / 2f);

            // Для плавности будем соединять через квадратичные кривые Q
            for (int i = 0; i < pointsCount; i++)
            {
                int next = (i + 1) % pointsCount;

                // Контрольная точка — середина между текущей и следующей
                float cx = (xs[i] + xs[next]) / 2f;
                float cy = (ys[i] + ys[next]) / 2f;

                // Q cx,cy  x,y 
                //sb.AppendFormat(CultureInfo.InvariantCulture,
                //    "Q{0},{1},{2},{3}",
                //    xs[i], ys[i], cx, cy
                //    );
                //Один знак после запятой для уменьшения размера строки.
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "Q{0:F1},{1:F1},{2:F1},{3:F1}",
                    xs[i], ys[i], cx, cy
                    );
            }



            sb.Append("Z"); // замыкаем путь
            return sb.ToString();
        }
        public string GenerateBlobPathDataAnimationValues(int valuesCount, int radius, int pointsCount, int variation, string? firstValue = null)
        {
            Random rnd = new();
            firstValue ??= GenerateBlobPathData(radius, pointsCount, variation, rnd);
            StringBuilder sb = new StringBuilder();
            sb.Append(firstValue);
            sb.Append(';');
            for (int i = 1; i < valuesCount; i++)
            {
                sb.Append(GenerateBlobPathData(radius, pointsCount, variation, rnd));
                sb.Append(';');
            }
            sb.Append(firstValue);

            return sb.ToString();

        }

        public string GenerateBlobPathFill(Random? rnd = null)
        {
            rnd ??= new();
            // Генерация гармоничных полупрозрачных неоновых тонов (Cyan -> Indigo -> Violet -> Magenta).
            int h = rnd.Next(180, 310); // Hue
            int s = rnd.Next(75, 95);   // Saturation
            int l = rnd.Next(45, 60);   // Lightness
            return $"hsla({h},{s}%,{l}%,0.65);";
        }
        public string GenerateBlobPathFillAnimationValues(int valuesCount)
        {
            Random rnd = new();
            StringBuilder sb = new StringBuilder();
            var firstValue = GenerateBlobPathFill(rnd);
            sb.Append(firstValue);
            for (int i = 1; i < valuesCount; i++)
                sb.Append(GenerateBlobPathFill(rnd) );
            sb.Append(firstValue );
            return sb.ToString();
        }
    }
}
