using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Socratic.PreviewGenerator;

Console.WriteLine("🚀 [Socratic Blazor Preview Generator] Initializing Headless HtmlRenderer...");

static string ResolvePreviewPagesDir()
{
    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current != null)
    {
        var rootCandidate = Path.Combine(current.FullName, "src", "Frontend", "Platform", "Shared", "DesignSystem", "Assets", "wwwroot", "preview");
        if (Directory.Exists(rootCandidate)) return rootCandidate;

        var platformCandidate = Path.Combine(current.FullName, "Shared", "DesignSystem", "Assets", "wwwroot", "preview");
        if (Directory.Exists(platformCandidate)) return platformCandidate;

        current = current.Parent;
    }
    return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../Shared/DesignSystem/Assets/wwwroot/preview"));
}

var defaultOutDir = ResolvePreviewPagesDir();

string outDir = defaultOutDir;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--out-dir" && i + 1 < args.Length)
    {
        outDir = Path.GetFullPath(args[i + 1]);
    }
}

Directory.CreateDirectory(outDir);
Console.WriteLine($"📁 Target Preview Directory: {outDir}\n");

var services = new ServiceCollection()
    .AddSocraticPreviewHost()
    .BuildServiceProvider();

var renderer = new SocraticHtmlPreviewRenderer(services);
var sw = Stopwatch.StartNew();
int renderedCount = 0;

// Registry of components to render into preview/pages/
var kioskParams = new Dictionary<string, object?>
{
    ["OrgId"] = "20240824-0000-0000-0000-000000000002",
    ["Slug"] = "demo"
};

var terminalParams = new Dictionary<string, object?>
{
    ["OrganizationId"] = "20240824-0000-0000-0000-000000000002"
};

var productParams = new Dictionary<string, object?>
{
    ["OrganizationId"] = "20240824-0000-0000-0000-000000000002",
    ["ProductId"] = "11111111-2222-3333-4444-555555555501",
    ["Slug"] = "demo-product"
};

var orderItemsParams = new Dictionary<string, object?>
{
    ["RouteOrderId"] = "11111111-2222-3333-4444-555555555501"
};

var componentRegistry = new (string Name, Func<Task<string>> RenderAction)[]
{
    // Retail / Point of Sale
    ("kiosk", () => renderer.RenderComponentAsync<Socratic.PointOfSale.Pages.Kiosk>(kioskParams)),
    ("terminal", () => renderer.RenderComponentAsync<Socratic.PointOfSale.Pages.Terminal>()),
    ("wishlist", () => renderer.RenderComponentAsync<Socratic.PointOfSale.Pages.WishList>()),
    ("product", () => renderer.RenderComponentAsync<Socratic.PointOfSale.Pages.ProductPage>(productParams)),

    // Portal / Landing
    ("landing", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Landing>()),
    ("about", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.About>()),
    ("docs", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Docs>()),
    ("help", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Help>()),
    ("qr", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Qr>()),
    ("analytics", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Analytics>()),
    ("player", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Player>()),
    ("3d", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.ModelViewer>()),

    // Portal / Identity
    ("signin", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignIn>()),
    ("signin-telegram", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignInTelegramCallback>()),
    ("signin-google", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignInGoogleCallback>()),

    // Portal / Organization
    ("home", () => renderer.RenderComponentAsync<Socratic.Organization.Pages.Home>()),

    // Retail / Checkout & Orders
    ("payment", () => renderer.RenderComponentAsync<Socratic.Checkout.Pages.Payment>()),
    ("orders", () => renderer.RenderComponentAsync<Socratic.Orders.Pages.OrdersPage>()),
    ("order-items", () => renderer.RenderComponentAsync<Socratic.Orders.Pages.OrderItemsPage>(orderItemsParams)),

    // Intelligence
    ("detector", () => renderer.RenderComponentAsync<Socratic.Vision.Pages.Detector>()),
    ("chat", () => renderer.RenderComponentAsync<Chat.Web.Pages.ChatPage>()),

    // Studio
    ("seat-designer", () => renderer.RenderComponentAsync<Socratic.SeatDesigner.Pages.SeatDesignerPage>()),
    ("qr-designer", () => renderer.RenderComponentAsync<Socratic.QrDesigner.Pages.QrDesignerPage>()),
    ("map", () => renderer.RenderComponentAsync<Map.Pages.MapPage>())
};

var nav = services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

foreach (var (name, renderAction) in componentRegistry)
{
    var compSw = Stopwatch.StartNew();
    try
    {
        nav?.SetUri(name == "landing" ? "" : name);
        var html = await renderAction();
        var targetFile = Path.Combine(outDir, $"{name}.html");
        await File.WriteAllTextAsync(targetFile, html);
        renderedCount++;
        Console.WriteLine($"  ✅ Generated: {name}.html ({compSw.ElapsedMilliseconds}ms) -> {targetFile}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"  ❌ Failed to render {name}: {ex.Message}");
    }
}

Console.WriteLine($"\n✨ Completed rendering {renderedCount} components in {sw.ElapsedMilliseconds}ms total.");
return 0;
