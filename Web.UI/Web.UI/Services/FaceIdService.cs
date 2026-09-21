#if FEATURE_BIOMETRICS || FEATURE_VISION
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Biometrics;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Web.UI.Services
{
    /// <summary>
    /// Промышленная реализация IFaceIdService на базе Qdrant Vector DB
    /// с защищенным жизненным циклом сессий калибровки и принципом Fail-Closed.
    /// </summary>
    public class FaceIdService : IFaceIdService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly ILogger<FaceIdService>? _logger;
        private const string CollectionName = "users_faces";

        // Потокобезопасные сессии регистрации (TTL 3 минуты)
        private class EnrollmentSession
        {
            public string UserId { get; set; } = string.Empty;
            public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(3);
            public int CurrentStage { get; set; } = 1; // 1 = Прямо, 2 = Влево, 3 = Вправо
            public DateTimeOffset LastStageChangedAt { get; set; } = DateTimeOffset.UtcNow;
            public List<float[]> Frames { get; } = new(3);
        }

        private static readonly ConcurrentDictionary<string, EnrollmentSession> _enrollmentSessions = new();

        public FaceIdService(QdrantClient qdrantClient, ILogger<FaceIdService>? logger = null)
        {
            _qdrantClient = qdrantClient;
            _logger = logger;
        }

        private async Task<bool> EnsureCollectionExistsAsync()
        {
            try
            {
                var collections = await _qdrantClient.ListCollectionsAsync();
                if (!collections.Contains(CollectionName))
                {
                    _logger?.LogInformation("[FaceIdService] Qdrant collection '{Collection}' does not exist. Creating with 512 dimensions (Cosine distance)...", CollectionName);
                    await _qdrantClient.CreateCollectionAsync(CollectionName, new VectorParams
                    {
                        Size = 512,
                        Distance = Distance.Cosine
                    });
                    _logger?.LogInformation("[FaceIdService] Qdrant collection '{Collection}' created successfully.", CollectionName);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[FaceIdService] Qdrant service is unavailable.");
                return false;
            }
        }

        public async Task<bool> RegisterFaceAsync(string userName, float[] embedding)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Идентификатор пользователя не должен быть пустым.", nameof(userName));
            if (embedding == null || embedding.Length != 512)
            {
                throw new ArgumentException("Эмбеддинг должен быть 512-мерным вектором.", nameof(embedding));
            }

            try
            {
                if (!await EnsureCollectionExistsAsync())
                {
                    _logger?.LogError("[FaceIdService] Cannot register face: Qdrant cluster unreachable (Fail-Closed).");
                    return false;
                }

                var pointId = Guid.TryParse(userName, out var parsedGuid) ? parsedGuid : Guid.NewGuid();
                var point = new PointStruct
                {
                    Id = pointId,
                    Vectors = embedding,
                    Payload =
                    {
                        ["user_name"] = userName
                    }
                };

                await _qdrantClient.UpsertAsync(CollectionName, new[] { point });
                _logger?.LogInformation("[FaceIdService] Successfully registered face vector for '{Identity}' in Qdrant (Point ID: {PointId})", userName, pointId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[FaceIdService] Failed to upsert face vector into Qdrant for '{Identity}'", userName);
                return false;
            }
        }

        public async Task<string?> SearchFaceAsync(float[] embedding, float threshold = 0.70f)
        {
            if (embedding == null || embedding.Length != 512)
            {
                throw new ArgumentException("Эмбеддинг должен быть 512-мерным вектором.", nameof(embedding));
            }

            try
            {
                if (!await EnsureCollectionExistsAsync())
                {
                    _logger?.LogWarning("[FaceIdService] Vector search failed: Qdrant is unavailable (Fail-Closed).");
                    return null;
                }

                var searchResults = await _qdrantClient.SearchAsync(
                    collectionName: CollectionName,
                    vector: embedding,
                    limit: 1
                );

                if (searchResults.Count > 0)
                {
                    var bestResult = searchResults[0];
                    _logger?.LogInformation("[FaceIdService] Qdrant search returned top result with score {Score:F4} (Required threshold: {Threshold:F2})",
                        bestResult.Score, threshold);

                    if (bestResult.Score >= threshold)
                    {
                        if (bestResult.Payload.TryGetValue("user_name", out var nameVal))
                        {
                            _logger?.LogInformation("[FaceIdService] Face matched in Qdrant with identity '{Identity}' (Score: {Score:F4})",
                                nameVal.StringValue, bestResult.Score);
                            return nameVal.StringValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[FaceIdService] Error during Qdrant SearchAsync.");
            }

            return null;
        }

        public async Task<bool> HasFaceRegisteredAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;

            try
            {
                if (!await EnsureCollectionExistsAsync()) return false;

                var pointId = Guid.TryParse(userName, out var parsedGuid) ? parsedGuid : Guid.Empty;
                if (pointId != Guid.Empty)
                {
                    var points = await _qdrantClient.RetrieveAsync(CollectionName, pointId);
                    if (points != null && points.Count > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "[FaceIdService] Error checking face status for '{UserName}'", userName);
            }

            return false;
        }

        public async Task<bool> DeleteFaceAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;

            try
            {
                if (!await EnsureCollectionExistsAsync()) return false;

                var pointId = Guid.TryParse(userName, out var parsedGuid) ? parsedGuid : Guid.Empty;
                if (pointId != Guid.Empty)
                {
                    await _qdrantClient.DeleteAsync(CollectionName, pointId);
                    _logger?.LogInformation("[FaceIdService] Deleted face vector for user '{UserName}' from Qdrant.", userName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[FaceIdService] Failed to delete face vector for user '{UserName}' from Qdrant.", userName);
            }

            return false;
        }

        public string StartEnrollmentSession(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));

            // Очищаем устаревшие сессии
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in _enrollmentSessions)
            {
                if (kvp.Value.ExpiresAt < now) _enrollmentSessions.TryRemove(kvp.Key, out _);
            }

            var sessionId = Guid.NewGuid().ToString("N");
            _enrollmentSessions[sessionId] = new EnrollmentSession { UserId = userId, CurrentStage = 1, LastStageChangedAt = DateTimeOffset.UtcNow };
            _logger?.LogInformation("[FaceIdService] Started 3D multi-pose enrollment session {SessionId} for User {UserId}", sessionId, userId);
            return sessionId;
        }

        public (bool Accepted, bool Completed, int Stage, string Message, float[]? MasterEmbedding, string? UserId) ProcessEnrollmentFrame(string sessionId, float[] frameEmbedding, float yawAngle = 0f)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || !_enrollmentSessions.TryGetValue(sessionId, out var session))
            {
                return (false, false, 0, "Сессия калибровки истекла или не найдена.", null, null);
            }

            if (session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _enrollmentSessions.TryRemove(sessionId, out _);
                return (false, false, 0, "Сессия калибровки истекла. Начните заново.", null, null);
            }

            if (frameEmbedding == null || frameEmbedding.Length != 512)
            {
                return (false, false, session.CurrentStage, "Некорректный вектор признаков кадра.", null, session.UserId);
            }

            var timeSinceLastStage = (DateTimeOffset.UtcNow - session.LastStageChangedAt).TotalMilliseconds;

            lock (session.Frames)
            {
                if (session.CurrentStage == 1)
                {
                    // Этап 1: Смотреть прямо (|Yaw| <= 9 градусов)
                    if (Math.Abs(yawAngle) > 9.0f)
                    {
                        return (false, false, 1, "Этап 1/3: 🎯 Пожалуйста, смотрите прямо в камеру...", null, session.UserId);
                    }

                    session.Frames.Add(frameEmbedding);
                    session.CurrentStage = 2;
                    session.LastStageChangedAt = DateTimeOffset.UtcNow;
                    return (true, false, 2, "Этап 1/3 пройден! 👈 Теперь плавно поверните голову влево...", null, session.UserId);
                }
                else if (session.CurrentStage == 2)
                {
                    if (timeSinceLastStage < 500)
                    {
                        return (false, false, 2, "👈 Поворачивайте голову влево...", null, session.UserId);
                    }

                    // Этап 2: Повернуть влево (Yaw < -7 градусов)
                    if (yawAngle > -7.0f)
                    {
                        return (false, false, 2, "Этап 2/3: 👈 Поверните голову слегка влево...", null, session.UserId);
                    }

                    session.Frames.Add(frameEmbedding);
                    session.CurrentStage = 3;
                    session.LastStageChangedAt = DateTimeOffset.UtcNow;
                    return (true, false, 3, "Этап 2/3 пройден! 👉 Теперь плавно поверните голову вправо...", null, session.UserId);
                }
                else if (session.CurrentStage == 3)
                {
                    if (timeSinceLastStage < 500)
                    {
                        return (false, false, 3, "👉 Поворачивайте голову вправо...", null, session.UserId);
                    }

                    // Этап 3: Повернуть вправо (Yaw > +7 градусов)
                    if (yawAngle < 7.0f)
                    {
                        return (false, false, 3, "Этап 3/3: 👉 Поверните голову слегка вправо...", null, session.UserId);
                    }

                    session.Frames.Add(frameEmbedding);

                    // Вычисляем 3D мастер-шаблон
                    float[] master = new float[512];
                    for (int f = 0; f < session.Frames.Count; f++)
                    {
                        var emb = session.Frames[f];
                        for (int i = 0; i < 512; i++) master[i] += emb[i];
                    }

                    // L2-нормализация
                    float norm = 0f;
                    for (int i = 0; i < 512; i++) norm += master[i] * master[i];
                    norm = (float)Math.Sqrt(norm);
                    if (norm > 0f)
                    {
                        for (int i = 0; i < 512; i++) master[i] /= norm;
                    }

                    var userId = session.UserId;
                    _enrollmentSessions.TryRemove(sessionId, out _);
                    return (true, true, 3, "✅ 3D-слепок лица успешно зафиксирован со всех ракурсов!", master, userId);
                }
            }

            return (false, false, session.CurrentStage, "Обработка кадра...", null, session.UserId);
        }

        public void CancelEnrollmentSession(string sessionId)
        {
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                _enrollmentSessions.TryRemove(sessionId, out _);
            }
        }

        public string? GetEnrollmentUserId(string sessionId)
        {
            if (!string.IsNullOrWhiteSpace(sessionId) && _enrollmentSessions.TryGetValue(sessionId, out var session))
            {
                return session.UserId;
            }
            return null;
        }
    }
}
#endif

