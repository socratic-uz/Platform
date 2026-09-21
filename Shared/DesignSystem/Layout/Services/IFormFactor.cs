namespace Shared.Services
{
    /// <summary>
    /// Интерфейс кроссплатформенной адаптации (Mobile, Desktop, Web).
    /// </summary>
    public interface IFormFactor
    {
        public string GetFormFactor();
        public string GetPlatform();
        public bool IsMobile();
        public bool IsDesktop();
        public bool IsWeb();
    }
}
