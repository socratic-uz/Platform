using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Domain.Interfaces.Biometrics
{
    /// <summary>
    /// Контракт для инкапсулированного распознавания, оценки качества и извлечения признаков лиц.
    /// Позволяет UI-модулям работать с биометрией без прямой зависимости от нативного ONNX runtime.
    /// </summary>
    public interface IFaceBiometricProcessor : IDisposable
    {
        bool IsSimulation { get; }

        List<FaceDetectionResult> DetectFaces(SKBitmap bitmap, float scoreThreshold = 0.5f);

        PadEvaluationResult EvaluateLiveness(SKBitmap bitmap, FaceDetectionResult face);

        float[] GenerateEmbedding(SKBitmap bitmap, FaceDetectionResult face);

        float EstimateYawAngle(FaceDetectionResult face);
    }
}
