using System;

namespace Shared.Services
{
    public class WebFormFactor : IFormFactor
    {
        public string GetFormFactor() => "Web";
        public string GetPlatform() => Environment.OSVersion.ToString();
        public bool IsMobile() => false;
        public bool IsDesktop() => true;
        public bool IsWeb() => true;
    }
}
