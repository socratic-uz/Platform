using System.Globalization;
using System.Text.Json;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;

namespace Native.UI
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, WindowSoftInputMode = SoftInput.AdjustResize, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity, IOnApplyWindowInsetsListener
    {
        private Android.Views.View? _topBarScrim;
        private Android.Views.View? _bottomBarScrim;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var defaultBg = Android.Graphics.Color.ParseColor("#0E1513");
            Window?.DecorView.SetBackgroundColor(defaultBg);
            UpdateLightBars(false);

            var contentView = FindViewById(Android.Resource.Id.Content) as FrameLayout;
            if (contentView is not null)
            {
                _topBarScrim = new Android.Views.View(this) { Clickable = false, Focusable = false };
                _topBarScrim.SetBackgroundColor(defaultBg);
                contentView.AddView(_topBarScrim, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, GravityFlags.Top));

                _bottomBarScrim = new Android.Views.View(this) { Clickable = false, Focusable = false };
                _bottomBarScrim.SetBackgroundColor(defaultBg);
                contentView.AddView(_bottomBarScrim, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, GravityFlags.Bottom));

                ViewCompat.SetOnApplyWindowInsetsListener(contentView, this);
                ViewCompat.RequestApplyInsets(contentView);
            }
        }

        public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View? v, WindowInsetsCompat? insets)
        {
            if (v is not null && insets is not null)
            {
                var bars = insets.GetInsets(
                    WindowInsetsCompat.Type.SystemBars() |
                    WindowInsetsCompat.Type.DisplayCutout() |
                    WindowInsetsCompat.Type.Ime());

                if (bars is not null)
                {
                    v.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);

                    if (_topBarScrim?.LayoutParameters is FrameLayout.LayoutParams topLp)
                    {
                        topLp.Height = bars.Top;
                        _topBarScrim.LayoutParameters = topLp;
                        _topBarScrim.TranslationY = -bars.Top;
                    }

                    if (_bottomBarScrim?.LayoutParameters is FrameLayout.LayoutParams bottomLp)
                    {
                        bottomLp.Height = bars.Bottom;
                        _bottomBarScrim.LayoutParameters = bottomLp;
                        _bottomBarScrim.TranslationY = bars.Bottom;
                    }
                }
            }

            return WindowInsetsCompat.Consumed!;
        }

        public void UpdateSystemBarColors(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var topHex = root.TryGetProperty("top", out var topProp) ? topProp.GetString() : null;
                var bottomHex = root.TryGetProperty("bottom", out var bottomProp) ? bottomProp.GetString() : null;
                var isLight = root.TryGetProperty("isLight", out var lightProp) && lightProp.GetBoolean();

                RunOnUiThread(() =>
                {
                    if (TryParseCssColor(topHex, out var topColor))
                        _topBarScrim?.SetBackgroundColor(topColor);

                    if (TryParseCssColor(bottomHex, out var bottomColor))
                    {
                        _bottomBarScrim?.SetBackgroundColor(bottomColor);
                        Window?.DecorView.SetBackgroundColor(bottomColor);
                    }

                    UpdateLightBars(isLight);
                });
            }
            catch
            {
            }
        }

        private void UpdateLightBars(bool isLight)
        {
            if (Window is null)
                return;

            var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
            if (controller is not null)
            {
                controller.AppearanceLightStatusBars = isLight;
                controller.AppearanceLightNavigationBars = isLight;
            }
        }

        private static bool TryParseCssColor(string? css, out Android.Graphics.Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(css))
                return false;

            css = css.Trim();
            if (css.StartsWith('#'))
            {
                color = Android.Graphics.Color.ParseColor(css);
                return true;
            }

            var start = css.IndexOf('(');
            var end = css.LastIndexOf(')');
            if (start < 0 || end <= start)
                return false;

            var parts = css[(start + 1)..end].Split([',', ' ', '/'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 &&
                byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) &&
                byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var g) &&
                byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
            {
                color = Android.Graphics.Color.Rgb(r, g, b);
                return true;
            }

            return false;
        }
    }
}
