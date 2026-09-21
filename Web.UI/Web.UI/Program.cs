global using static SharedKernel.ValueObjects.EnvironmentVariables;
global using System.Linq;
global using System.Linq.Expressions;

using static SharedKernel.Options.MinioOptions;
using static SharedKernel.Options.OpenAiOptions;
using static SharedKernel.Options.GoogleOptions;
using System.Text;
#if FEATURE_BIOMETRICS || FEATURE_VISION
using Biometrics.Processor;
#endif
#if FEATURE_FFMPEG
using FFmpegProcessor;
#endif
#if FEATURE_CHAT
using Chat.Web;
#endif
#if FEATURE_MARKDOWN
using Markdown.Web;
#endif
using CameraStreamer.Web;
using System.Text.Json.Serialization;

using Domain.Interfaces;
using Aspire.Qdrant.Client;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.AI;
using OpenAI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.SemanticKernel.Connectors.Qdrant;

using Qdrant.Client;
using Qdrant.Client.Grpc;

using SharedKernel.ValueObjects;

using Shared;
using Shared.Pages;
using SkiaSharp;
using Shared.Services;
using Web.UI.Components;
using Web.UI.Services;
using Web.UI.Services.Ingestion;
using Smart.Web;
using SharedKernel.Abstractions;
using Domain.Entities;
using Web.UI.Endpoints;
#if FEATURE_MAP
using Map;
#endif
#if FEATURE_QRDESIGNER
using Socratic.QrDesigner;
#endif
#if FEATURE_RECEIPTPRINTER || FEATURE_POS || FEATURE_KIOSK || FEATURE_COMMERCE
using Socratic.ReceiptPrinter;
#endif
using CameraStreamer.Web;

using static Shared.Helpers.FileHelper;

Console.OutputEncoding = Encoding.UTF8;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

MinIOUrl = builder.Configuration[MINIO_URL] ?? MinIOUrl;

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents().AddHubOptions(o => o.MaximumReceiveMessageSize = 100_000_000)
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Register Feature Modules explicitly for AOT-safe dynamic micro-frontend routing and services
foreach (var module in new Domain.Interfaces.IFeatureModule[] {
#if FEATURE_LANDING
    new Socratic.Landing.LandingFeatureModule(),
#endif
#if FEATURE_POS
    new Socratic.POS.POSFeatureModule(),
#endif
#if FEATURE_KIOSK
    new Socratic.Kiosk.KioskFeatureModule(),
#endif
#if FEATURE_COMMERCE
    new Socratic.Commerce.CommerceFeatureModule(),
#endif
#if FEATURE_CHAT
    new Chat.ChatFeatureModule(),
#endif
#if FEATURE_VISION
    new Socratic.Vision.VisionFeatureModule(),
#endif
#if FEATURE_CHECKOUT
    new Socratic.Checkout.CheckoutFeatureModule(),
#endif
#if FEATURE_ORDERS
    new Socratic.Orders.OrdersFeatureModule(),
#endif
#if FEATURE_ORGANIZATION
    new Socratic.Organization.OrganizationFeatureModule(),
#endif
#if FEATURE_IDENTITY
    new Socratic.Identity.IdentityFeatureModule(),
#endif
#if FEATURE_MAP
    new Map.MapFeatureModule(),
#endif
#if FEATURE_QRDESIGNER
    new QrDesigner.QrDesignerFeatureModule(),
#endif
#if FEATURE_SEATDESIGNER
    new SeatDesigner.SeatDesignerFeatureModule(),
#endif
})
{
    Domain.Interfaces.FeatureRegistry.Register(module);
}
builder.Services.AddRegisteredFeatureServices(builder.Configuration);

#region AI
try
{
    // 1. Google AI / Gemini configuration (Priority)
    var googleApiKey = builder.Configuration[GOOGLE_API_KEY]
        ?? builder.Configuration["GOOGLE_API_KEY"];

    var googleModel = builder.Configuration[GOOGLE_MODEL]
        ?? builder.Configuration["GOOGLE_MODEL"];

    var googleEmbeddingModel = builder.Configuration[GOOGLE_EMBEDDING_MODEL]
        ?? builder.Configuration["GOOGLE_EMBEDDING_MODEL"]
        ?? "text-embedding-004";

    var googleEndpoint = builder.Configuration[GOOGLE_ENDPOINT]
        ?? builder.Configuration["GOOGLE_ENDPOINT"];

    if (!string.IsNullOrEmpty(googleApiKey) && !string.IsNullOrEmpty(googleModel) && !string.IsNullOrEmpty(googleEndpoint))
    {
        var endpoint = new Uri(googleEndpoint);
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(googleApiKey), new OpenAIClientOptions { Endpoint = endpoint });
        var chatClient = new Web.UI.Services.GoogleChatClient(openAiClient.GetChatClient(googleModel), googleModel, endpoint);
        builder.Services.AddSingleton<IChatClient>(chatClient);

        var embeddingGenerator = openAiClient.GetEmbeddingClient(googleEmbeddingModel).AsIEmbeddingGenerator();
        builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(embeddingGenerator);
        Console.WriteLine($"[AI Init] Google initialized with chat model '{googleModel}' and embedding model '{googleEmbeddingModel}'.");
    }
    else
    {
        // 2. OpenAI / Fallback configuration
        var openAiConnection = builder.Configuration[OPENAI_CONNECTION];
        var key = openAiConnection?.Split(';').FirstOrDefault(s => s.StartsWith("Key=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
        var endpointStr = openAiConnection?.Split(';').FirstOrDefault(s => s.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];

        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(endpointStr))
        {
            var endpoint = new Uri(endpointStr);

            if (endpoint.Host.Contains("models.inference.ai.azure.com", StringComparison.OrdinalIgnoreCase) ||
                endpoint.Host.Contains("api.openai.com", StringComparison.OrdinalIgnoreCase) ||
                endpoint.Host.Contains("googleapis.com", StringComparison.OrdinalIgnoreCase) ||
                !endpoint.Host.Contains("openai.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(key), new OpenAIClientOptions { Endpoint = endpoint });
                var chatClient = new Web.UI.Services.OpenAiInferenceChatClient(openAiClient.GetChatClient("gpt-4o-mini"));
                builder.Services.AddSingleton<IChatClient>(chatClient);

                var embeddingGenerator = openAiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
                builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(embeddingGenerator);
            }
            else
            {
                var openai = builder.AddAzureOpenAIClient(OPENAI_CONNECTION, s =>
                {
                    s.Key = key;
                    s.Endpoint = endpoint;
                });
                openai.AddChatClient("gpt-4o-mini").UseFunctionInvocation();
                openai.AddEmbeddingGenerator("text-embedding-3-small");
            }
        }
    }

    builder.AddQdrantClient();
}
catch (Exception ex)
{
    Console.WriteLine($"[AI Init] AI/Qdrant initialization skipped: {ex.Message}");
}

builder.Services.AddQdrantCollection<Guid, IngestedChunk>("data-chatapp-chunks");
builder.Services.AddQdrantCollection<Guid, IngestedDocument>("data-chatapp-documents");
builder.Services.AddScoped<DataIngestor>();
#if FEATURE_BIOMETRICS || FEATURE_VISION
builder.Services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, FaceIdService>();
builder.Services.AddBiometricProcessorServices();
builder.Services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, SupportSessionProvider>();
#endif

#if FEATURE_FFMPEG
builder.Services.AddFFmpegProcessorServices();
#endif

#if FEATURE_CHAT && FEATURE_MARKDOWN
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddSingleton<ISemanticSearch>(sp => sp.GetRequiredService<SemanticSearch>());
#endif
#endregion

#if FEATURE_MAP
builder.Services.AddMapServices();
#endif

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ISettingsManager, Web.UI.Services.ServerSettingsManager>();
builder.Services.AddScoped<IPasskeyService, PasskeyService>();
#if FEATURE_MARKDOWN
builder.Services.AddMarkdownServices();
#endif
builder.Services.AddSmartWebServices();
#if FEATURE_QRDESIGNER
builder.Services.AddQrDesignerServices();
#endif
#if FEATURE_RECEIPTPRINTER || FEATURE_POS || FEATURE_KIOSK || FEATURE_COMMERCE
builder.Services.AddReceiptPrinterServices();
#endif
builder.Services.AddCameraStreamer();

// Server overrides
#if FEATURE_CHAT
builder.Services.AddScoped<Chat.Abstractions.IChatService, Web.UI.Services.ServerChatService>();
#endif
builder.Services.AddScoped<Domain.Interfaces.Pages.IDetectorService, Web.UI.Services.ServerDetectorService>();
builder.Services.AddLocalization().AddScoped<IStringLocalizer, StringLocalizer>();


AuthenticationExtensions.AddAuthentication(builder.Services);


builder.Services.AddGrpcClients(builder.Configuration);
builder.Services.AddSingleton<IFormFactor, WebFormFactor>();

builder.Services.AddHttpClient();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, Shared.Serialization.AotJsonContext.Default);
});

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "ru-RU", "ru", "uz-Latn-UZ", "uz", "en-US", "en" };
    options.SetDefaultCulture("ru-RU")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapProtomapsTileEndpoints();
#if FEATURE_BIOMETRICS || FEATURE_VISION
app.MapFaceIdEndpoints();
#endif
app.MapAuthEndpoints();


if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var supportedCultures = new[] { "ru-RU", "ru", "uz-Latn-UZ", "uz", "en-US", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ru-RU")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// Configure MIME types for static assets (especially RCL JavaScript modules)
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".json"] = "application/json";

app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    ContentTypeProvider = provider
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies([
        typeof(Web.UI.Client._Imports).Assembly,
        typeof(Shared._Imports).Assembly,
        .. Domain.Interfaces.FeatureRegistry.GetFeatureAssemblies()
    ]);

app.MapHub<Web.UI.Hubs.RemoteSupportHub>("/hubs/remotesupport");
#if FEATURE_CHAT
app.MapHub<Web.UI.Hubs.ChatHub>("/hubs/chat");
#endif
app.MapHub<Web.UI.Hubs.DetectorHub>("/hubs/detector");

app.MapGet("/api/culture/set", (string culture, string? redirectUri, HttpContext httpContext) =>
{
    if (!string.IsNullOrWhiteSpace(culture))
    {
        httpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
    }
    return Results.LocalRedirect(string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri);
}).DisableAntiforgery();

app.MapSeoEndpoints();
#if FEATURE_FFMPEG
app.MapStreamingEndpoints();
#endif
app.MapDocsEndpoints();

try
{
    var docsPath = DocsEndpoints.GetDocsRoot(builder.Environment);

    await DataIngestor.IngestDataAsync(
        app.Services,
        new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")),
        new MarkdownDirectorySource(docsPath, searchSubdirectories: true));
}
catch (Exception) { }

app.Run();