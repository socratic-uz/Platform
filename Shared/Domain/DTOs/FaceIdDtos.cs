using System;

namespace Domain.DTOs
{
    public class FaceRegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public class FaceRegisterResponse
    {
        public bool Success { get; set; }
    }

    public class FaceSearchRequest
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public float Threshold { get; set; } = 0.65f;
    }

    public class FaceSearchResponse
    {
        public string? MatchedUserName { get; set; }
    }

    public class FaceFrameProcessRequest
    {
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
        public float ScoreThreshold { get; set; } = 0.5f;
    }

    public class FaceFrameProcessResponse
    {
        public bool FaceDetected { get; set; }
        public float Score { get; set; }
        public float Left { get; set; }
        public float Top { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public class FaceStatusResponse
    {
        public bool Configured { get; set; }
    }

    // Zero-Exposure Server-Controlled Enrollment & Verification DTOs
    public class FaceEnrollStartRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class FaceEnrollStartResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public int RequiredStages { get; set; } = 3;
        public int CurrentStage { get; set; } = 1;
        public string Instruction { get; set; } = string.Empty;
    }

    public class FaceEnrollFrameRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    }

    public class FaceEnrollFrameResponse
    {
        public bool Accepted { get; set; }
        public bool Completed { get; set; }
        public int CurrentStage { get; set; }
        public int RequiredStages { get; set; } = 3;
        public string Message { get; set; } = string.Empty;
    }

    public class FaceVerifyLoginRequest
    {
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
        public string? UserId { get; set; }
    }

    public class FaceVerifyLoginResponse
    {
        public bool Success { get; set; }
        public bool FaceDetected { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
