using System;
using SkiaSharp;

namespace Domain.Interfaces.Biometrics
{
    /// <summary>
    /// Представляет результат детекции лица на изображении.
    /// </summary>
    public class FaceDetectionResult
    {
        public SKRect BoundingBox { get; set; }
        public float Score { get; set; }
        public SKPoint[] Landmarks { get; set; } = Array.Empty<SKPoint>();
    }

    /// <summary>
    /// Результат оценки живости субъекта и защиты от спуфинга (ISO/IEC 30107-3 PAD).
    /// </summary>
    public class PadEvaluationResult
    {
        public bool IsLive { get; set; }
        public float SpoofProbability { get; set; }
        public float LivenessScore { get; set; }
        public string AttackType { get; set; } = "None";
        public string Message { get; set; } = string.Empty;
    }
}
