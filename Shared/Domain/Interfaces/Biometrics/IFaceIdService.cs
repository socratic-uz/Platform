using System;
using System.Threading.Tasks;

namespace Domain.Interfaces.Biometrics;

/// <summary>
/// Интерфейс для взаимодействия с биометрическим поиском и защищенными сессиями калибровки лица.
/// </summary>
public interface IFaceIdService
{
    /// <summary>
    /// Регистрирует биометрический шаблон лица под именем пользователя.
    /// </summary>
    Task<bool> RegisterFaceAsync(string userName, float[] embedding);

    /// <summary>
    /// Ищет совпадение лица в базе данных и возвращает имя пользователя.
    /// </summary>
    Task<string?> SearchFaceAsync(float[] embedding, float threshold = 0.65f);

    /// <summary>
    /// Проверяет наличие зарегистрированного биометрического шаблона для пользователя.
    /// </summary>
    Task<bool> HasFaceRegisteredAsync(string userName);

    /// <summary>
    /// Удаляет биометрический шаблон пользователя из базы данных.
    /// </summary>
    Task<bool> DeleteFaceAsync(string userName);

    /// <summary>
    /// Инициирует серверную сессию регистрации лица (3D Multi-Pose Enrollment).
    /// </summary>
    string StartEnrollmentSession(string userId);

    /// <summary>
    /// Принимает кадр калибровки в серверную сессию с проверкой ракурса (Yaw) и возвращает прогресс или итоговый мастер-шаблон.
    /// </summary>
    (bool Accepted, bool Completed, int Stage, string Message, float[]? MasterEmbedding, string? UserId) ProcessEnrollmentFrame(string sessionId, float[] frameEmbedding, float yawAngle = 0f);

    /// <summary>
    /// Отменяет активную сессию калибровки.
    /// </summary>
    void CancelEnrollmentSession(string sessionId);

    /// <summary>
    /// Получает идентификатор пользователя для указанной сессии калибровки.
    /// </summary>
    string? GetEnrollmentUserId(string sessionId);
}
