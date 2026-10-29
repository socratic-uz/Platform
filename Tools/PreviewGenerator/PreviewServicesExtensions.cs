using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using Microsoft.AspNetCore.Components.Authorization;
using Socratic.Platform.Layout.Extensions;
using Shared.Services;
using SharedKernel.Abstractions;

namespace Socratic.PreviewGenerator;

public static class PreviewServicesExtensions
{
    public static IServiceCollection AddSocraticPreviewHost(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        services.AddSingleton<NavigationManager, FakeNavigationManager>();
        services.AddSingleton<IJSRuntime, NullJsRuntime>();

        services.AddSingleton<IStringLocalizer, FakeStringLocalizer>();
        services.AddSingleton(typeof(IStringLocalizer<>), typeof(FakeStringLocalizer<>));
        services.AddSingleton<IStringLocalizerFactory, FakeStringLocalizerFactory>();
        services.AddSingleton<Microsoft.AspNetCore.Components.Web.IErrorBoundaryLogger, NullErrorBoundaryLogger>();

        services.AddHttpClient();
        services.AddSingleton(new HttpClient(new FastPreviewHttpMessageHandler()));
        services.AddAuthorizationCore();
        services.AddLayoutServices();
        services.AddScoped<PersistentComponentState>(sp => CreatePersistentComponentState());

        services.AddScoped<Domain.Interfaces.Hardware.IPosPinPadService, Infrastructure.Gateways.Hardware.MockPosPinPadService>();
        services.AddScoped<Domain.Interfaces.Hardware.ICashAcceptorService, Infrastructure.Gateways.Hardware.MockCashAcceptorService>();
        services.AddScoped<Domain.Interfaces.Hardware.IReceiptPrinterService, Infrastructure.Gateways.Receipt.BrowserReceiptPrinterService>();
        services.AddScoped<ISettingsManager, Shared.Services.SettingsManager>();
        services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();

        var dummyChannel = Grpc.Net.Client.GrpcChannel.ForAddress("http://localhost:5000", new Grpc.Net.Client.GrpcChannelOptions
        {
            HttpHandler = new ImmediateUnavailableHttpMessageHandler()
        });
        services.AddSingleton(new Shopping.Application.Protos.WishService.WishServiceClient(dummyChannel));
        services.AddSingleton(new Paying.Application.Protos.PaymentService.PaymentServiceClient(dummyChannel));
        services.AddSingleton(new Identifying.Application.Protos.AuthService.AuthServiceClient(dummyChannel));
        services.AddSingleton(new Shopping.Application.Protos.OrganizationService.OrganizationServiceClient(dummyChannel));
        services.AddSingleton(new Shopping.Application.Protos.ProductService.ProductServiceClient(dummyChannel));
        services.AddSingleton(new Shopping.Application.Protos.LayoutService.LayoutServiceClient(dummyChannel));
        services.AddSingleton(new Ordering.Application.Protos.OrderService.OrderServiceClient(dummyChannel));
        services.AddSingleton(new Ordering.Application.Protos.OrderItemService.OrderItemServiceClient(dummyChannel));
        services.AddSingleton(new Identifying.Application.Protos.UserService.UserServiceClient(dummyChannel));
        services.AddSingleton(new Identifying.Application.Protos.RoleService.RoleServiceClient(dummyChannel));
        services.AddSingleton(new Identifying.Application.Protos.ProfileService.ProfileServiceClient(dummyChannel));

        Infrastructure.Core.GrpcExtension.AddGrpcClientWrappers(services);
        services.AddScoped<SharedKernel.Abstractions.IServiceClientFactory, Infrastructure.Services.ServiceClientFactory>();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationManager());

        global::Map.MapServiceCollectionExtensions.AddMapServices(services);
        Socratic.QrDesigner.QrDesignerServiceCollectionExtensions.AddQrDesignerServices(services);
        FFmpegProcessor.DependencyInjection.AddFFmpegProcessorServices(services);
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(new Socratic.Platform.Composition.Services.SocraticHostEnvironment { EnvironmentName = "Development", ApplicationName = "Socratic" });
        services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, Infrastructure.Gateways.Biometrics.ClientFaceIdService>();
        services.AddScoped<Domain.Interfaces.Biometrics.IFaceBiometricProcessor, MockFaceBiometricProcessor>();
        services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, Infrastructure.Gateways.Support.ClientSupportSessionProvider>();
        services.AddSingleton<Domain.Interfaces.Commerce.IProductExperienceUiProvider, Socratic.Commerce.ProductExperienceUiProvider>();
        services.AddScoped<Chat.Abstractions.IChatService, MockChatService>();
        services.AddScoped<Domain.Interfaces.Pages.IDetectorService, MockDetectorService>();
        services.AddScoped<Shared.Services.IPasskeyService, MockPasskeyService>();
        services.AddScoped<Infrastructure.Gateways.Identity.IPasskeyService, MockInfrastructurePasskeyService>();

        return services;
    }

    private static PersistentComponentState CreatePersistentComponentState()
    {
        var ctors = typeof(PersistentComponentState).GetConstructors(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);

        foreach (var ctor in ctors)
        {
            var pars = ctor.GetParameters();
            if (pars.Length == 0)
                return (PersistentComponentState)ctor.Invoke(null);
            if (pars.Length == 2)
            {
                return (PersistentComponentState)ctor.Invoke(new object[] { new Dictionary<string, byte[]>(), new List<Func<Task>>() });
            }
        }

        return (PersistentComponentState)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(PersistentComponentState));
    }
}

public sealed class FakeNavigationManager : NavigationManager
{
    public FakeNavigationManager()
    {
        Initialize("http://localhost:6080/", "http://localhost:6080/");
    }

    public void SetUri(string pageName)
    {
        var relative = pageName.TrimStart('/');
        Uri = string.IsNullOrEmpty(relative)
            ? BaseUri
            : new Uri(new Uri(BaseUri), relative).ToString();
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        Uri = uri;
    }
}

public sealed class NullJsRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        string identifier, 
        object?[]? args) => ValueTask.FromResult(default(TValue)!);

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        string identifier, 
        CancellationToken cancellationToken, 
        object?[]? args) => ValueTask.FromResult(default(TValue)!);
}

public sealed class FakeStringLocalizer : IStringLocalizer
{
    private static readonly System.Resources.ResourceManager RuManager = new("Assets.Resources.ResourceRu", typeof(global::Assets.Constants.Classes).Assembly);
    private static readonly System.Resources.ResourceManager UzManager = new("Assets.Resources.ResourceUz", typeof(global::Assets.Constants.Classes).Assembly);
    private static readonly System.Resources.ResourceManager EnManager = new("Assets.Resources.ResourceEn", typeof(global::Assets.Constants.Classes).Assembly);

    internal static string Resolve(string name)
        => RuManager.GetString(name)
        ?? UzManager.GetString(name)
        ?? EnManager.GetString(name)
        ?? name;

    public LocalizedString this[string name]
    {
        get
        {
            var val = Resolve(name);
            return new(name, val, val == name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var val = Resolve(name);
            return new(name, string.Format(val, arguments), val == name);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

public sealed class FakeStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name]
    {
        get
        {
            var val = FakeStringLocalizer.Resolve(name);
            return new(name, val, val == name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var val = FakeStringLocalizer.Resolve(name);
            return new(name, string.Format(val, arguments), val == name);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

public sealed class FakeStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => new FakeStringLocalizer();
    public IStringLocalizer Create(string baseName, string location) => new FakeStringLocalizer();
}

public sealed class NullErrorBoundaryLogger : Microsoft.AspNetCore.Components.Web.IErrorBoundaryLogger
{
    public ValueTask LogErrorAsync(Exception exception) => ValueTask.CompletedTask;
}

public sealed class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity())));
}

public sealed class MockChatService : Chat.Abstractions.IChatService
{
    public async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<Chat.Abstractions.ChatMessageDto> messages, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return "Здравствуйте! Чем я могу помочь вам сегодня?";
        await Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetSuggestionsAsync(IEnumerable<Chat.Abstractions.ChatMessageDto> messages, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(new[] { "Как открыть киоск?", "Подключение терминала", "28 режимов коммерции" });
    }
}

public sealed class MockDetectorService : Domain.Interfaces.Pages.IDetectorService
{
    public Task<Domain.Interfaces.Pages.FaceRegistrationResultDto> RegisterFaceAsync(string name, byte[] imageBytes, CancellationToken cancellationToken = default)
        => Task.FromResult(new Domain.Interfaces.Pages.FaceRegistrationResultDto(true, "Success", name));

    public Task<Domain.Interfaces.Pages.DetectionResultDto[]> ProcessFrameAsync(byte[] imageBytes, float threshold = 0.5f, CancellationToken cancellationToken = default)
        => Task.FromResult(Array.Empty<Domain.Interfaces.Pages.DetectionResultDto>());
}

public sealed class MockInfrastructurePasskeyService : Infrastructure.Gateways.Identity.IPasskeyService
{
    public ValueTask<bool> IsSupportedAsync() => ValueTask.FromResult(false);
    public ValueTask<string?> RegisterAsync(string userId, string userName, string challengeBase64) => ValueTask.FromResult<string?>(null);
    public ValueTask<string?> AuthenticateAsync(string challengeBase64) => ValueTask.FromResult<string?>(null);
}

public sealed class MockPasskeyService : Shared.Services.IPasskeyService
{
    public ValueTask<bool> IsSupportedAsync() => ValueTask.FromResult(false);
    public ValueTask<string?> RegisterAsync(string userId, string userName, string challengeBase64) => ValueTask.FromResult<string?>(null);
    public ValueTask<string?> AuthenticateAsync(string challengeBase64) => ValueTask.FromResult<string?>(null);
}

public sealed class MockFaceBiometricProcessor : Domain.Interfaces.Biometrics.IFaceBiometricProcessor
{
    public bool IsSimulation => true;
    public List<Domain.Interfaces.Biometrics.FaceDetectionResult> DetectFaces(SkiaSharp.SKBitmap bitmap, float scoreThreshold = 0.5f) => [];
    public Domain.Interfaces.Biometrics.PadEvaluationResult EvaluateLiveness(SkiaSharp.SKBitmap bitmap, Domain.Interfaces.Biometrics.FaceDetectionResult face) => new Domain.Interfaces.Biometrics.PadEvaluationResult { IsLive = true, LivenessScore = 0.99f };
    public float[] GenerateEmbedding(SkiaSharp.SKBitmap bitmap, Domain.Interfaces.Biometrics.FaceDetectionResult face) => Array.Empty<float>();
    public float EstimateYawAngle(Domain.Interfaces.Biometrics.FaceDetectionResult face) => 0f;
    public void Dispose() { }
}

public sealed class ImmediateUnavailableHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Static preview generator: backend connection disabled.", null, System.Net.HttpStatusCode.ServiceUnavailable);
    }
}

public sealed class FastPreviewHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";
        if (url.Contains("/docs/") || url.EndsWith(".md"))
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("# Socratic Documentation\n\nPreview documentation content.")
            });
        }
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        });
    }
}
