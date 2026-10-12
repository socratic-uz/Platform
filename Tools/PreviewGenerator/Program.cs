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
        var candidate = Path.Combine(current.FullName, "src", "Frontend", "Platform", "Shared", "DesignSystem", "Assets", "wwwroot", "preview", "pages");
        if (Directory.Exists(candidate)) return candidate;
        current = current.Parent;
    }
    return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Shared/DesignSystem/Assets/wwwroot/preview/pages"));
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
    ("qr", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.QR>()),
    ("analytics", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Analytics>()),
    ("player", () => renderer.RenderComponentAsync<Socratic.Landing.Pages.Player>()),
    ("3d", () => renderer.RenderComponentAsync<Socratic.Landing.Pages._3d>()),

    // Portal / Identity
    ("signin", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignIn>()),
    ("signin-telegram", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignInTelegramCallback>()),
    ("signin-google", () => renderer.RenderComponentAsync<Socratic.Identity.Pages.SignInGoogleCallback>()),

    // Portal / Organization
    ("home", () => renderer.RenderComponentAsync<Socratic.Organization.Pages.Home>()),

    // Retail / Checkout & Orders
    ("payment", () => renderer.RenderComponentAsync<Socratic.Checkout.Pages.Payment>()),
    ("orders", () => renderer.RenderComponentAsync<Socratic.Orders.Pages.Orders.Index>()),
    ("order-items", () => renderer.RenderComponentAsync<Socratic.Orders.Pages.OrderItems.Index>(orderItemsParams)),

    // Intelligence
    ("detector", () => renderer.RenderComponentAsync<Socratic.Vision.Pages.Detector>()),
    ("chat", () => renderer.RenderComponentAsync<Chat.Web.Pages.ChatPage>()),

    // Studio
    ("seat-designer", () => renderer.RenderComponentAsync<SeatDesigner.Pages.SeatDesignerPage>()),
    ("qr-designer", () => renderer.RenderComponentAsync<QrDesigner.Pages.QrDesignerPage>()),
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

// Internal test components demonstrating M3 structure and zero-flexbox CSS Grid layout
public class SampleCatalogPreview : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "catalog-shell m3-grid");

        // Header
        builder.OpenElement(2, "header");
        builder.AddAttribute(3, "class", "m3-top-app-bar");
        builder.AddContent(4, "Socratic Commerce Catalog");
        builder.CloseElement();

        // Main layout
        builder.OpenElement(5, "main");
        builder.AddAttribute(6, "class", "m3-grid");

        // Sidebar
        builder.OpenElement(7, "aside");
        builder.AddAttribute(8, "class", "catalog-sidebar");
        builder.AddContent(9, "Sidebar Filters");
        builder.CloseElement();

        // Content
        builder.OpenElement(10, "section");
        builder.AddAttribute(11, "class", "m3-content");
        
        for (int i = 1; i <= 3; i++)
        {
            builder.OpenElement(12 + i, "div");
            builder.AddAttribute(13 + i, "class", "m3-card");
            builder.AddContent(14 + i, $"Product Item #{i}");
            builder.CloseElement();
        }

        builder.CloseElement(); // section
        builder.CloseElement(); // main
        builder.CloseElement(); // div.catalog-shell
    }
}

public class SampleDashboardPreview : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "dashboard-shell m3-grid");

        builder.OpenElement(2, "header");
        builder.AddAttribute(3, "class", "m3-top-app-bar");
        builder.AddContent(4, "POS Analytics Dashboard");
        builder.CloseElement();

        builder.OpenElement(5, "section");
        builder.AddAttribute(6, "class", "m3-content m3-grid");
        builder.AddContent(7, "Revenue Metrics & Sales Widgets");
        builder.CloseElement();

        builder.CloseElement();
    }
}

public class KioskPreview : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "app-grid");

        // 1. Header (Sticky)
        builder.OpenElement(2, "header");
        builder.AddAttribute(3, "class", "app-header header-sticky");

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "header-grid-layout");

        // Brand zone
        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "header-start-zone");
        builder.OpenElement(8, "div");
        builder.AddAttribute(9, "class", "kiosk-brand");
        builder.OpenElement(10, "h2");
        builder.AddAttribute(11, "class", "kiosk-brand-name");
        builder.AddContent(12, "Demo Store — AI Kiosk");
        builder.CloseElement();
        builder.CloseElement(); // kiosk-brand
        builder.CloseElement(); // header-start-zone

        // Search zone
        builder.OpenElement(13, "div");
        builder.AddAttribute(14, "class", "header-center-zone");
        builder.OpenElement(15, "md-outlined-text-field");
        builder.AddAttribute(16, "placeholder", "Поиск блюд и напитков...");
        builder.CloseElement();
        builder.CloseElement();

        // Actions zone
        builder.OpenElement(17, "div");
        builder.AddAttribute(18, "class", "header-end-zone");
        builder.OpenElement(19, "md-outlined-button");
        builder.AddContent(20, "RU");
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement(); // header-grid-layout
        builder.CloseElement(); // header

        // 2. Categories chip-set
        builder.OpenElement(21, "nav");
        builder.AddAttribute(22, "class", "kiosk-categories-nav");
        builder.OpenElement(23, "md-chip-set");
        
        string[] categories = ["Все блюда", "Пицца & Бургеры", "Кофе & Чай", "Десерты", "Бизнес-ланчи"];
        int seq = 24;
        foreach (var cat in categories)
        {
            builder.OpenElement(seq++, "md-filter-chip");
            builder.AddAttribute(seq++, "label", cat);
            builder.CloseElement();
        }
        builder.CloseElement(); // md-chip-set
        builder.CloseElement(); // nav

        // 3. Catalog Grid
        builder.OpenElement(seq++, "main");
        builder.AddAttribute(seq++, "class", "kiosk-catalog m3-grid");

        for (int i = 1; i <= 6; i++)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "m3-card product-card");

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "m3-card-header");
            builder.OpenElement(seq++, "h3");
            builder.AddContent(seq++, $"Комбо-сет #{i}");
            builder.CloseElement();
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "m3-price-tag");
            builder.AddContent(seq++, $"{i * 25000:N0} UZS");
            builder.CloseElement();
            builder.CloseElement(); // m3-card-header

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "m3-card-actions");
            builder.OpenElement(seq++, "md-filled-button");
            builder.AddContent(seq++, "В корзину");
            builder.CloseElement();
            builder.CloseElement(); // m3-card-actions

            builder.CloseElement(); // m3-card
        }

        builder.CloseElement(); // main.kiosk-catalog

        // 4. Cart Floating Summary
        builder.OpenElement(seq++, "aside");
        builder.AddAttribute(seq++, "class", "kiosk-cart-summary m3-surface");
        builder.OpenElement(seq++, "span");
        builder.AddContent(seq++, "Корзина: 0 позиций");
        builder.CloseElement();
        builder.OpenElement(seq++, "md-filled-button");
        builder.AddContent(seq++, "Оформить заказ");
        builder.CloseElement();
        builder.CloseElement(); // aside

        builder.CloseElement(); // app-grid
    }
}
