using System.Collections.Generic;
using Domain.Interfaces.Biometrics;

namespace Shared.Services
{
    public class ClientSupportSessionProvider : ISupportSessionProvider
    {
        private static readonly List<string> _sessions = new();

        public List<string> GetActiveSessions()
        {
            lock (_sessions)
            {
                return new List<string>(_sessions);
            }
        }

        public void RegisterSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            lock (_sessions)
            {
                if (!_sessions.Contains(sessionId)) _sessions.Add(sessionId);
            }
        }

        public void UnregisterSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            lock (_sessions)
            {
                _sessions.Remove(sessionId);
            }
        }
    }
}
