using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;

using Shared;
using Shared.Services;
using Socratic.Platform.Composition.Extensions;
#if FEATURE_MARKDOWN
using Markdown.Web;
#endif
#if FEATURE_MAP
using Map;
#endif
using Smart.Web;
using Domain.Interfaces;

#if FEATURE_BIOMETRICS || FEATURE_VISION
using Biometrics.Processor;
#endif
#if FEATURE_FFMPEG
using FFmpegProcessor;
#endif

using static SharedKernel.Options.MinioOptions;
using static SharedKernel.ValueObjects.EnvironmentVariables;
using static Shared.Helpers.FileHelper;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
MinIOUrl = builder.Configuration[MINIO_URL] ?? MinIOUrl;

// 1. Unified Composition Application Services (Layout, all 13 features, Design System, gRPC, Localization)
builder.Services.AddCompositionServices(builder.Configuration);

// 2. WebAssembly platform-specific adaptations
builder.Services.AddSingleton<IFormFactor, WebFormFactor>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddScoped<Domain.Interfaces.Biometrics.IFaceIdService, Infrastructure.Gateways.Biometrics.ClientFaceIdService>();
builder.Services.AddScoped<Domain.Interfaces.Biometrics.ISupportSessionProvider, Infrastructure.Gateways.Support.ClientSupportSessionProvider>();
builder.Services.AddScoped<Domain.Interfaces.Hardware.IReceiptPrinterService, Infrastructure.Gateways.Receipt.BrowserReceiptPrinterService>();
builder.Services.AddScoped<Domain.Interfaces.Hardware.ICameraStreamerUiProvider, CameraStreamer.Web.CameraStreamerUiProvider>();

await builder.Build().RunAsync();
