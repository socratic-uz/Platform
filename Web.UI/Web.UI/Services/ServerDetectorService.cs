using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Pages;
#if FEATURE_BIOMETRICS || FEATURE_VISION
using Domain.Interfaces.Biometrics;
#endif

namespace Web.UI.Services
{
    public class ServerDetectorService : IDetectorService
    {
#if FEATURE_BIOMETRICS || FEATURE_VISION
        private readonly IFaceIdService? _faceIdService;

        public ServerDetectorService(IFaceIdService? faceIdService = null)
        {
            _faceIdService = faceIdService;
        }
#else
        public ServerDetectorService()
        {
        }
#endif

        public async Task<FaceRegistrationResultDto> RegisterFaceAsync(string name, byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new FaceRegistrationResultDto(false, "Name cannot be empty", null);
            }

#if FEATURE_BIOMETRICS || FEATURE_VISION
            if (_faceIdService != null)
            {
                float[] embedding = Array.Empty<float>();
                var success = await _faceIdService.RegisterFaceAsync(name, embedding);
                return new FaceRegistrationResultDto(success, success ? $"Registered '{name}' successfully." : "Registration failed.", name);
            }
#endif

            return new FaceRegistrationResultDto(true, $"Simulated registration for '{name}'.", name);
        }

        public Task<DetectionResultDto[]> ProcessFrameAsync(byte[] imageBytes, float threshold = 0.5f, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<DetectionResultDto>());
        }
    }
}
