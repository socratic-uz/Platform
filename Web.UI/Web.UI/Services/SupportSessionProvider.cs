#if FEATURE_BIOMETRICS || FEATURE_VISION
using Domain.Interfaces.Biometrics;
using System.Collections.Generic;
using System.Linq;
using Web.UI.Hubs;

namespace Web.UI.Services
{
    /// <summary>
    /// Предоставляет список активных сессий удаленной поддержки, считывая их из хаба SignalR.
    /// </summary>
    public class SupportSessionProvider : ISupportSessionProvider
    {
        public List<string> GetActiveSessions()
        {
            return RemoteSupportHub.SessionChannels.Keys.ToList();
        }
    }
}
#endif

