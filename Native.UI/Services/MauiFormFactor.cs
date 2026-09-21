using Microsoft.Maui.Devices;
using Shared.Services;

namespace Native.UI.Services
{
    public class MauiFormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return DeviceInfo.Idiom.ToString();
        }

        public string GetPlatform()
        {
            return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
        }

        public bool IsMobile()
        {
            return DeviceInfo.Idiom == DeviceIdiom.Phone;
        }

        public bool IsDesktop()
        {
            return DeviceInfo.Idiom == DeviceIdiom.Desktop;
        }

        public bool IsWeb() => false;
    }
}
