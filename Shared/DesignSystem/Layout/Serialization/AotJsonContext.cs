using System.Text.Json.Serialization;

namespace Shared.Serialization
{
    /// <summary>
    /// Source-generated JsonSerializerContext for Native AOT zero-reflection JSON operations across Blazor UI.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        GenerationMode = JsonSourceGenerationMode.Default,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(float[]))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Domain.DTOs.FaceRegisterRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceRegisterResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceSearchRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceSearchResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceFrameProcessRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceFrameProcessResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceStatusResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceEnrollStartRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceEnrollStartResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceEnrollFrameRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceEnrollFrameResponse))]
    [JsonSerializable(typeof(Domain.DTOs.FaceVerifyLoginRequest))]
    [JsonSerializable(typeof(Domain.DTOs.FaceVerifyLoginResponse))]
    [JsonSerializable(typeof(Domain.DTOs.PasskeyRegisterDto))]
    [JsonSerializable(typeof(Domain.DTOs.PasskeyLoginDto))]
    [JsonSerializable(typeof(Domain.DTOs.TelegramAuthDto))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.OrganizationSubscription))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.SubscriptionTier))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.AdBannerItem))]
    [JsonSerializable(typeof(List<SharedKernel.ValueObjects.AdBannerItem>))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.AdNetworkProvider))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.AdNetworkSettings))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.PromotionPlanType))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.PromotionPaymentGatewayType))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.ProductPromotionOrder))]
    [JsonSerializable(typeof(SharedKernel.ValueObjects.ProductPromotionPlanDefinition))]
    [JsonSerializable(typeof(List<SharedKernel.ValueObjects.ProductPromotionPlanDefinition>))]
    public partial class AotJsonContext : JsonSerializerContext
    {
    }
}
