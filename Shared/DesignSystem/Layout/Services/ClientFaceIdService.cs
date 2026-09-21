using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Interfaces.Biometrics;
using Microsoft.Extensions.Logging;
using Domain.DTOs;
using Shared.Serialization;

namespace Shared.Services
{
    public class ClientFaceIdService : IFaceIdService
    {
        private readonly HttpClient? _httpClient;
        private readonly ILogger<ClientFaceIdService>? _logger;

        public ClientFaceIdService(HttpClient? httpClient = null, ILogger<ClientFaceIdService>? logger = null)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> RegisterFaceAsync(string userName, float[] embedding)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name must not be empty.", nameof(userName));
            if (embedding == null) throw new ArgumentNullException(nameof(embedding));

            if (_httpClient != null)
            {
                try
                {
                    var req = new FaceRegisterRequest { UserName = userName, Embedding = embedding };
                    var response = await _httpClient.PostAsJsonAsync("/api/faceid/register", req, AotJsonContext.Default.FaceRegisterRequest);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "[ClientFaceIdService] Failed to register face via HTTP.");
                }
            }

            return false;
        }

        public async Task<string?> SearchFaceAsync(float[] embedding, float threshold = 0.65f)
        {
            if (embedding == null || _httpClient == null) return null;

            try
            {
                var req = new FaceSearchRequest { Embedding = embedding, Threshold = threshold };
                var response = await _httpClient.PostAsJsonAsync("/api/faceid/search", req, AotJsonContext.Default.FaceSearchRequest);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync(AotJsonContext.Default.FaceSearchResponse);
                    return res?.MatchedUserName;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[ClientFaceIdService] Server face search request failed.");
            }

            return null;
        }

        public async Task<bool> HasFaceRegisteredAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName) || _httpClient == null) return false;

            try
            {
                var response = await _httpClient.GetAsync($"/api/faceid/status?userId={Uri.EscapeDataString(userName)}");
                if (response.IsSuccessStatusCode)
                {
                    var status = await response.Content.ReadFromJsonAsync(AotJsonContext.Default.FaceStatusResponse);
                    return status?.Configured ?? false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "[ClientFaceIdService] Failed to query face status from server for user '{UserName}'", userName);
            }

            return false;
        }

        public async Task<bool> DeleteFaceAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName) || _httpClient == null) return false;

            try
            {
                var response = await _httpClient.DeleteAsync($"/api/faceid?userId={Uri.EscapeDataString(userName)}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "[ClientFaceIdService] Failed to delete face from server for user '{UserName}'", userName);
            }

            return false;
        }

        public string StartEnrollmentSession(string userId)
        {
            return Guid.NewGuid().ToString("N");
        }

        public (bool Accepted, bool Completed, int Stage, string Message, float[]? MasterEmbedding, string? UserId) ProcessEnrollmentFrame(string sessionId, float[] frameEmbedding, float yawAngle = 0f)
        {
            return (true, true, 3, "OK", frameEmbedding, null);
        }

        public void CancelEnrollmentSession(string sessionId)
        {
        }

        public string? GetEnrollmentUserId(string sessionId)
        {
            return null;
        }
    }
}
