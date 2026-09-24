using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Domain.Interfaces.Pages;

namespace Infrastructure.Gateways.Vision
{
    public class RemoteDetectorService : IDetectorService
    {
        private readonly string _hubUrl;

        public RemoteDetectorService(NavigationManager navManager, IConfiguration? configuration = null)
        {
            _hubUrl = HubUrlResolver.ResolveHubUrl(navManager, configuration, "/hubs/detector");
        }

        public RemoteDetectorService(IConfiguration configuration)
        {
            _hubUrl = HubUrlResolver.ResolveHubUrl(null, configuration, "/hubs/detector");
        }

        public RemoteDetectorService(string hubUrl)
        {
            _hubUrl = string.IsNullOrWhiteSpace(hubUrl) ? "/hubs/detector" : hubUrl;
        }

        public RemoteDetectorService()
            : this(string.Empty)
        {
        }

        private async Task<HubConnection?> ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                await connection.StartAsync(cancellationToken);
                return connection;
            }
            catch
            {
                return null;
            }
        }

        public async Task<FaceRegistrationResultDto> RegisterFaceAsync(string name, byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            await using var connection = await ConnectAsync(cancellationToken);
            if (connection == null)
            {
                return new FaceRegistrationResultDto(false, $"Не удалось подключиться к серверу детектора ({_hubUrl}).", null);
            }

            return await connection.InvokeAsync<FaceRegistrationResultDto>("RegisterFace", name, imageBytes, cancellationToken);
        }

        public async Task<DetectionResultDto[]> ProcessFrameAsync(byte[] imageBytes, float threshold = 0.5f, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await ConnectAsync(cancellationToken);
                if (connection == null)
                {
                    return Array.Empty<DetectionResultDto>();
                }

                return await connection.InvokeAsync<DetectionResultDto[]>("ProcessFrame", imageBytes, threshold, cancellationToken);
            }
            catch
            {
                return Array.Empty<DetectionResultDto>();
            }
        }
    }
}
