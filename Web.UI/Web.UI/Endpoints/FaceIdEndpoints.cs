#if FEATURE_BIOMETRICS || FEATURE_VISION
using Domain.Interfaces.Biometrics;
using Biometrics.Processor.Services;
using IFaceIdService = Domain.Interfaces.Biometrics.IFaceIdService;
using IFaceBiometricProcessor = Domain.Interfaces.Biometrics.IFaceBiometricProcessor;
using Identifying.Application.Protos;
using SkiaSharp;
using Domain.DTOs;
using Shared.Serialization;

namespace Web.UI.Endpoints;

public static class FaceIdEndpoints
{
    public static void MapFaceIdEndpoints(this IEndpointRouteBuilder app)
    {
        // 🔹 Face ID WebAssembly Client Sync Endpoints (Qdrant Vector DB)
        app.MapPost("/api/faceid/enroll/start", (
            FaceEnrollStartRequest request,
            IFaceIdService faceIdService) =>
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return Results.BadRequest("UserId is required.");
            }
            var sessionId = faceIdService.StartEnrollmentSession(request.UserId);
            var response = new FaceEnrollStartResponse
            {
                SessionId = sessionId,
                CurrentStage = 1,
                RequiredStages = 3,
                Instruction = "Этап 1/3: Посмотрите прямо в камеру..."
            };
            return Results.Json(response, AotJsonContext.Default.FaceEnrollStartResponse);
        });

        app.MapPost("/api/faceid/enroll/frame", async (
            FaceEnrollFrameRequest request,
            IFaceBiometricProcessor processor,
            IFaceIdService faceIdService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || request.ImageBytes == null || request.ImageBytes.Length == 0)
            {
                return Results.BadRequest(new FaceEnrollFrameResponse
                {
                    Accepted = false,
                    Message = "Кадр не получен."
                });
            }

            try
            {
                using var bitmap = SKBitmap.Decode(request.ImageBytes);
                if (bitmap == null)
                {
                    return Results.Json(new FaceEnrollFrameResponse
                    {
                        Accepted = false,
                        Message = "Некорректный формат кадра."
                    }, AotJsonContext.Default.FaceEnrollFrameResponse);
                }

                var faces = processor.DetectFaces(bitmap, 0.50f);
                if (faces.Count == 0)
                {
                    return Results.Json(new FaceEnrollFrameResponse
                    {
                        Accepted = false,
                        Message = "Расположите лицо по центру кадра и убедитесь, что оно полностью видно..."
                    }, AotJsonContext.Default.FaceEnrollFrameResponse);
                }

                var face = faces[0];

                // 🛡️ Проверка живости субъекта (ISO/IEC 30107-3 PAD: защита от экранов и фото)
                var padResult = processor.EvaluateLiveness(bitmap, face);
                if (!padResult.IsLive)
                {
                    return Results.Json(new FaceEnrollFrameResponse
                    {
                        Accepted = false,
                        Message = padResult.Message
                    }, AotJsonContext.Default.FaceEnrollFrameResponse);
                }

                var embedding = processor.GenerateEmbedding(bitmap, face);
                if (embedding == null || embedding.Length != 512)
                {
                    return Results.Json(new FaceEnrollFrameResponse
                    {
                        Accepted = false,
                        Message = "Ошибка извлечения биометрических признаков."
                    }, AotJsonContext.Default.FaceEnrollFrameResponse);
                }

                var yawAngle = processor.EstimateYawAngle(face);
                var (accepted, completed, stage, msg, masterEmbedding, userId) = faceIdService.ProcessEnrollmentFrame(request.SessionId, embedding, yawAngle);

                if (completed && masterEmbedding != null && !string.IsNullOrWhiteSpace(userId))
                {
                    var registered = await faceIdService.RegisterFaceAsync(userId, masterEmbedding);
                    if (!registered)
                    {
                        completed = false;
                        msg = "Не удалось сохранить биометрию в базе векторов.";
                    }
                }

                var response = new FaceEnrollFrameResponse
                {
                    Accepted = accepted,
                    Completed = completed,
                    CurrentStage = stage,
                    RequiredStages = 3,
                    Message = msg
                };

                return Results.Json(response, AotJsonContext.Default.FaceEnrollFrameResponse);
            }
            catch (Exception ex)
            {
                return Results.Json(new FaceEnrollFrameResponse
                {
                    Accepted = false,
                    Message = $"Ошибка обработки кадра: {ex.Message}"
                }, AotJsonContext.Default.FaceEnrollFrameResponse);
            }
        });

        app.MapPost("/api/faceid/verify-login", async (
            FaceVerifyLoginRequest request,
            IFaceBiometricProcessor processor,
            IFaceIdService faceIdService,
            AuthService.AuthServiceClient authClient,
            CancellationToken ct) =>
        {
            if (request.ImageBytes == null || request.ImageBytes.Length == 0)
            {
                return Results.BadRequest(new FaceVerifyLoginResponse
                {
                    Success = false,
                    FaceDetected = false,
                    Message = "Кадр не получен."
                });
            }

            try
            {
                using var bitmap = SKBitmap.Decode(request.ImageBytes);
                if (bitmap == null)
                {
                    return Results.BadRequest(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = false,
                        Message = "Некорректный формат кадра."
                    });
                }

                var faces = processor.DetectFaces(bitmap, 0.50f);
                if (faces.Count == 0)
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = false,
                        Message = "Лицо не обнаружено в кадре."
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }

                var face = faces[0];
                var padResult = processor.EvaluateLiveness(bitmap, face);
                if (!padResult.IsLive)
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = true,
                        Message = padResult.Message
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }

                var embedding = processor.GenerateEmbedding(bitmap, face);
                if (embedding == null || embedding.Length != 512)
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = true,
                        Message = "Не удалось извлечь вектор признаков."
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }

                var recognizedUserId = await faceIdService.SearchFaceAsync(embedding, 0.40f);
                if (string.IsNullOrWhiteSpace(recognizedUserId))
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = true,
                        Message = "Лицо не распознано в системе."
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }

                var reply = await authClient.AuthenticateWithFaceIdAsync(new FaceIdCredentials
                {
                    UserId = recognizedUserId,
                    Vector = { embedding }
                }, cancellationToken: ct);

                if (reply != null && !string.IsNullOrWhiteSpace(reply.Token))
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = true,
                        FaceDetected = true,
                        Token = reply.Token,
                        UserId = recognizedUserId,
                        Message = "Успешно распознано! Выполняется вход..."
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }
                else
                {
                    return Results.Json(new FaceVerifyLoginResponse
                    {
                        Success = false,
                        FaceDetected = true,
                        UserId = recognizedUserId,
                        Message = "Ошибка выпуска токена авторизации."
                    }, AotJsonContext.Default.FaceVerifyLoginResponse);
                }
            }
            catch (Exception ex)
            {
                return Results.Json(new FaceVerifyLoginResponse
                {
                    Success = false,
                    FaceDetected = false,
                    Message = $"Ошибка верификации: {ex.Message}"
                }, AotJsonContext.Default.FaceVerifyLoginResponse);
            }
        });

        app.MapPost("/api/faceid/register", async (
            FaceRegisterRequest request,
            IFaceIdService faceIdService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || request.Embedding == null || request.Embedding.Length == 0)
            {
                return Results.BadRequest(new FaceRegisterResponse { Success = false });
            }
            var ok = await faceIdService.RegisterFaceAsync(request.UserName, request.Embedding);
            return Results.Json(new FaceRegisterResponse { Success = ok }, AotJsonContext.Default.FaceRegisterResponse);
        });

        app.MapPost("/api/faceid/search", async (
            FaceSearchRequest request,
            IFaceIdService faceIdService,
            CancellationToken ct) =>
        {
            if (request.Embedding == null || request.Embedding.Length == 0)
            {
                return Results.BadRequest(new FaceSearchResponse { MatchedUserName = null });
            }
            var match = await faceIdService.SearchFaceAsync(request.Embedding, request.Threshold);
            return Results.Json(new FaceSearchResponse { MatchedUserName = match }, AotJsonContext.Default.FaceSearchResponse);
        });

        app.MapPost("/api/faceid/process-frame", (
            FaceFrameProcessRequest request,
            IFaceBiometricProcessor processor) =>
        {
            if (request.ImageBytes == null || request.ImageBytes.Length == 0)
            {
                return Results.BadRequest(new FaceFrameProcessResponse { FaceDetected = false });
            }

            try
            {
                using var bitmap = SKBitmap.Decode(request.ImageBytes);
                if (bitmap == null)
                {
                    return Results.BadRequest(new FaceFrameProcessResponse { FaceDetected = false });
                }

                var faces = processor.DetectFaces(bitmap, request.ScoreThreshold);
                if (faces.Count == 0)
                {
                    return Results.Json(new FaceFrameProcessResponse { FaceDetected = false }, AotJsonContext.Default.FaceFrameProcessResponse);
                }

                var face = faces[0];
                var embedding = processor.GenerateEmbedding(bitmap, face);

                if (embedding == null || embedding.Length != 512)
                {
                    return Results.Json(new FaceFrameProcessResponse { FaceDetected = false }, AotJsonContext.Default.FaceFrameProcessResponse);
                }

                var response = new FaceFrameProcessResponse
                {
                    FaceDetected = true,
                    Score = face.Score,
                    Left = face.BoundingBox.Left,
                    Top = face.BoundingBox.Top,
                    Width = face.BoundingBox.Width,
                    Height = face.BoundingBox.Height,
                    Embedding = embedding
                };

                return Results.Json(response, AotJsonContext.Default.FaceFrameProcessResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Face frame processing failed: {ex.Message}");
                return Results.Json(new FaceFrameProcessResponse { FaceDetected = false }, AotJsonContext.Default.FaceFrameProcessResponse);
            }
        });

        app.MapGet("/api/faceid/status", async (
            string? userId,
            IFaceIdService faceIdService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Json(new FaceStatusResponse { Configured = false }, AotJsonContext.Default.FaceStatusResponse);
            }
            var exists = await faceIdService.HasFaceRegisteredAsync(userId);
            return Results.Json(new FaceStatusResponse { Configured = exists }, AotJsonContext.Default.FaceStatusResponse);
        });

        app.MapDelete("/api/faceid", async (
            string? userId,
            IFaceIdService faceIdService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.BadRequest();
            }
            var deleted = await faceIdService.DeleteFaceAsync(userId);
            return Results.Ok(new { success = deleted });
        });
    }
}
#endif

