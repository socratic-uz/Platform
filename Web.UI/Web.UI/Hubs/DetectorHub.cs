using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Domain.Interfaces.Pages;

namespace Web.UI.Hubs
{
    public class DetectorHub : Hub
    {
        private readonly IDetectorService _detectorService;

        public DetectorHub(IDetectorService detectorService)
        {
            _detectorService = detectorService;
        }

        public async Task<FaceRegistrationResultDto> RegisterFace(string name, byte[] imageBytes)
        {
            return await _detectorService.RegisterFaceAsync(name, imageBytes);
        }

        public async Task<DetectionResultDto[]> ProcessFrame(byte[] imageBytes, float threshold)
        {
            return await _detectorService.ProcessFrameAsync(imageBytes, threshold);
        }
    }
}
