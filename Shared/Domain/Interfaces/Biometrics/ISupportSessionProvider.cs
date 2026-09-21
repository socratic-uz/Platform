using System.Collections.Generic;

namespace Domain.Interfaces.Biometrics;

/// <summary>
/// Интерфейс для получения списка активных удаленных сессий поддержки.
/// </summary>
public interface ISupportSessionProvider
{
    /// <summary>
    /// Возвращает список идентификаторов активных сессий.
    /// </summary>
    List<string> GetActiveSessions();
}
