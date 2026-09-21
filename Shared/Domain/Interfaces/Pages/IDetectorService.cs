using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces.Pages
{
    public record FaceRegistrationResultDto(bool Success, string Message, string? UserRegistered);
    public record DetectionResultDto(string Label, float Confidence, float X, float Y, float Width, float Height);

    public interface IDetectorService
    {
        Task<FaceRegistrationResultDto> RegisterFaceAsync(string name, byte[] imageBytes, CancellationToken cancellationToken = default);
        Task<DetectionResultDto[]> ProcessFrameAsync(byte[] imageBytes, float threshold = 0.5f, CancellationToken cancellationToken = default);
    }
}
