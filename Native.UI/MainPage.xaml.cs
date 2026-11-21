using Microsoft.AspNetCore.Components.WebView;

namespace Native.UI
{
    public partial class MainPage : Microsoft.Maui.Controls.ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
        }

        private static void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
        {
#if IOS || MACCATALYST
            e.WebView.AllowsBackForwardNavigationGestures = true;
#elif ANDROID
            e.WebView.Settings.JavaScriptEnabled = true;
            e.WebView.Settings.MediaPlaybackRequiresUserGesture = false;
            e.WebView.Settings.SetGeolocationEnabled(true);
            e.WebView.Settings.UserAgentString = $"{e.WebView.Settings.UserAgentString} AndroidApp";
            e.WebView.SetWebChromeClient(new AppWebChromeClient(e.WebView.WebChromeClient));
#endif
        }

#if ANDROID
        private sealed class AppWebChromeClient(Android.Webkit.WebChromeClient? inner) : Android.Webkit.WebChromeClient
        {
            public override void OnGeolocationPermissionsShowPrompt(string? origin, Android.Webkit.GeolocationPermissions.ICallback? callback)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    callback?.Invoke(origin, status == PermissionStatus.Granted, false);
                });
            }

            public override void OnPermissionRequest(Android.Webkit.PermissionRequest? request)
            {
                if (request is null)
                    return;

                var resources = request.GetResources() ?? [];
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var granted = new List<string>();
                    foreach (var res in resources)
                    {
                        if (res == Android.Webkit.PermissionRequest.ResourceVideoCapture)
                        {
                            var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
                            if (cam != PermissionStatus.Granted)
                                cam = await Permissions.RequestAsync<Permissions.Camera>();
                            if (cam == PermissionStatus.Granted)
                                granted.Add(res);
                        }
                        else if (res == Android.Webkit.PermissionRequest.ResourceAudioCapture)
                        {
                            var mic = await Permissions.CheckStatusAsync<Permissions.Microphone>();
                            if (mic != PermissionStatus.Granted)
                                mic = await Permissions.RequestAsync<Permissions.Microphone>();
                            if (mic == PermissionStatus.Granted)
                                granted.Add(res);
                        }
                    }

                    if (granted.Count > 0)
                        request.Grant(granted.ToArray());
                    else
                        request.Deny();
                });
            }

            public override bool OnJsPrompt(
                Android.Webkit.WebView? view,
                string? url,
                string? message,
                string? defaultValue,
                Android.Webkit.JsPromptResult? result)
            {
                const string prefix = "__SYSTEM_BARS__:";
                if (message is not null && message.StartsWith(prefix, StringComparison.Ordinal))
                {
                    if (Platform.CurrentActivity is MainActivity mainActivity)
                    {
                        mainActivity.UpdateSystemBarColors(message[prefix.Length..]);
                    }
                    result?.Confirm("");
                    return true;
                }

                return inner?.OnJsPrompt(view, url, message, defaultValue, result)
                    ?? base.OnJsPrompt(view, url, message, defaultValue, result);
            }

            public override bool OnShowFileChooser(
                Android.Webkit.WebView? webView,
                Android.Webkit.IValueCallback? filePathCallback,
                FileChooserParams? fileChooserParams)
                => inner?.OnShowFileChooser(webView, filePathCallback, fileChooserParams)
                   ?? base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
        }
#endif

        protected override bool OnBackButtonPressed()
        {
            if (TryGoBackInWebView())
                return true;

#if ANDROID
            if (Platform.CurrentActivity?.MoveTaskToBack(false) == true)
                return true;
#endif
            return base.OnBackButtonPressed();
        }

        internal bool TryGoBackInWebView()
        {
#if ANDROID
            if (blazorWebView.Handler?.PlatformView is Android.Webkit.WebView androidWebView && androidWebView.CanGoBack())
            {
                androidWebView.GoBack();
                return true;
            }
#elif WINDOWS
            if (blazorWebView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 webView2 && webView2.CanGoBack)
            {
                webView2.GoBack();
                return true;
            }
#elif IOS || MACCATALYST
            if (blazorWebView.Handler?.PlatformView is WebKit.WKWebView wkWebView && wkWebView.CanGoBack)
            {
                wkWebView.GoBack();
                return true;
            }
#endif
            return false;
        }
    }
}
