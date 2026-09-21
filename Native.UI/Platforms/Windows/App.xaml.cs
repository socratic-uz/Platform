using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace Native.UI.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        private static readonly string LogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Socratic", "Logs");

        public App()
        {
            try
            {
                var webViewFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Socratic", "WebView2");
                Directory.CreateDirectory(webViewFolder);
                Directory.CreateDirectory(LogDir);
                Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", webViewFolder, EnvironmentVariableTarget.Process);
            }
            catch { }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                WriteCrashLog($"Unhandled AppDomain exception: {e.ExceptionObject}");
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                WriteCrashLog($"Unobserved Task exception: {e.Exception}");
            };
            this.UnhandledException += (s, e) =>
            {
                WriteCrashLog($"Unhandled WinUI exception: {e.Exception}");
                e.Handled = true;
            };
            this.InitializeComponent();
        }

        private static void WriteCrashLog(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                File.WriteAllText(Path.Combine(LogDir, "crash.log"), message);
            }
            catch { }
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
