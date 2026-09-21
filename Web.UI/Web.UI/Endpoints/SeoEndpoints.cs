using System.Collections.Concurrent;
using System.Text;
using SharedKernel.Abstractions;
using Domain.Entities;
using Shared.Helpers;

namespace Web.UI.Endpoints;

public static class SeoEndpoints
{
    private static string? _cachedSitemapXml;
    private static DateTime _sitemapCachedAt = DateTime.MinValue;
    private static readonly SemaphoreSlim _sitemapLock = new(1, 1);
    private static readonly ConcurrentDictionary<string, (string Content, DateTime CachedAt)> _feedCache = new();

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    public static void MapSeoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/robots.txt", () =>
        {
            const string content =
@"User-agent: *
Allow: /
Allow: /kiosk
Allow: /kiosk/
Allow: /store/
Allow: /_content/
Disallow: /terminal
Disallow: /orders
Disallow: /order-items
Disallow: /signin
Disallow: /signin-google-callback
Disallow: /signin-telegram-callback
Disallow: /hubs/
Disallow: /api/

Sitemap: https://socratic.uz/sitemap.xml";

            return Results.Content(content, "text/plain", Encoding.UTF8);
        }).DisableAntiforgery();

        app.MapGet("/ads.txt", () =>
        {
            const string content = "google.com, pub-3226044876387611, DIRECT, f08c47fec0942fa0\n";
            return Results.Content(content, "text/plain", Encoding.UTF8);
        }).DisableAntiforgery();

        app.MapGet("/sitemap.xml", async (
            IServiceClientFactory clientFactory,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (_cachedSitemapXml != null && (DateTime.UtcNow - _sitemapCachedAt).TotalHours < 1)
            {
                return Results.Content(_cachedSitemapXml, "application/xml", Encoding.UTF8);
            }

            await _sitemapLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedSitemapXml != null && (DateTime.UtcNow - _sitemapCachedAt).TotalHours < 1)
                {
                    return Results.Content(_cachedSitemapXml, "application/xml", Encoding.UTF8);
                }

                var sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                if (context.Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = "https://socratic.uz";
                }

                var nowStr = DateTime.UtcNow.ToString("yyyy-MM-dd");

                void AddUrl(string path, string priority, string changefreq)
                {
                    sb.AppendLine("  <url>");
                    sb.AppendLine($"    <loc>{baseUrl}{path}</loc>");
                    sb.AppendLine($"    <lastmod>{nowStr}</lastmod>");
                    sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
                    sb.AppendLine($"    <priority>{priority}</priority>");
                    sb.AppendLine("  </url>");
                }

                AddUrl("/", "1.0", "daily");
                AddUrl("/home", "0.9", "daily");
                AddUrl("/about", "0.7", "monthly");
                AddUrl("/docs", "0.8", "weekly");
                AddUrl("/kiosk", "0.9", "daily");

                try
                {
                    var orgService = clientFactory.GetClient<Organization>();
                    var prodService = clientFactory.GetClient<Product>();

                    var orgs = new List<Organization>();
                    try
                    {
                        var orgCall = orgService.Read(new Organization());
                        while (await orgCall.ResponseStream.MoveNext(cancellationToken))
                        {
                            if (orgCall.ResponseStream.Current?.Id != null)
                            {
                                orgs.Add(orgCall.ResponseStream.Current);
                            }
                        }
                    }
                    catch { }

                    if (orgs.Count == 0)
                    {
                        orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.DemoId, Name = "Demo" });
                        orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.SocraticId, Name = "Socratic" });
                    }

                    foreach (var org in orgs)
                    {
                        var orgSlug = SlugHelper.GenerateSlug(org.Name);
                        AddUrl($"/kiosk/{org.Id}/{orgSlug}", "0.9", "daily");

                        var prodCount = 0;
                        try
                        {
                            var prodCall = prodService.Read(new Product { OrganizationId = org.Id });
                            while (await prodCall.ResponseStream.MoveNext(cancellationToken))
                            {
                                var prod = prodCall.ResponseStream.Current;
                                if (prod?.Id != null)
                                {
                                    prodCount++;
                                    var slug = SlugHelper.GenerateSlug(prod.Name);
                                    AddUrl($"/kiosk/{org.Id}/product/{prod.Id}/{slug}", "0.8", "weekly");
                                }
                            }
                        }
                        catch { }

                        if (prodCount == 0)
                        {
                            if (org.Id == SharedKernel.Extensions.ObjectIdExtension.DemoId)
                            {
                                AddUrl($"/kiosk/{org.Id}/product/b1a10001-0000-0000-0000-000000000001/{SlugHelper.GenerateSlug("Узбекский Плов Ташкент")}", "0.8", "weekly");
                                AddUrl($"/kiosk/{org.Id}/product/b1a10001-0000-0000-0000-000000000002/{SlugHelper.GenerateSlug("Персональный Конфигуратор Пиццы")}", "0.8", "weekly");
                                AddUrl($"/kiosk/{org.Id}/product/b1a10001-0000-0000-0000-000000000003/{SlugHelper.GenerateSlug("VIP Билет на Концерт")}", "0.8", "weekly");
                            }
                            else if (org.Id == SharedKernel.Extensions.ObjectIdExtension.SocraticId)
                            {
                                AddUrl($"/kiosk/{org.Id}/product/b1a10002-0000-0000-0000-000000000001/{SlugHelper.GenerateSlug("Тариф Бизнес Про Сети и Ритейл")}", "0.8", "weekly");
                                AddUrl($"/kiosk/{org.Id}/product/b1a10002-0000-0000-0000-000000000002/{SlugHelper.GenerateSlug("Заявка на подключение и регистрацию организации")}", "0.8", "weekly");
                                AddUrl($"/kiosk/{org.Id}/product/b1a10002-0000-0000-0000-000000000003/{SlugHelper.GenerateSlug("Комплект POS терминал и Киоск самообслуживания")}", "0.8", "weekly");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Sitemap] Error generating sitemap: {ex.Message}");
                }

                sb.AppendLine("</urlset>");
                _cachedSitemapXml = sb.ToString();
                _sitemapCachedAt = DateTime.UtcNow;
                return Results.Content(_cachedSitemapXml, "application/xml", Encoding.UTF8);
            }
            finally
            {
                _sitemapLock.Release();
            }
        }).DisableAntiforgery();

        app.MapGet("/feeds/yandex/{orgId?}", async (
            string? orgId,
            IServiceClientFactory clientFactory,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var cleanOrgId = orgId?.Trim();
            if (!string.IsNullOrEmpty(cleanOrgId) && cleanOrgId.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                cleanOrgId = cleanOrgId[..^4];
            }

            var cacheKey = $"yandex_{cleanOrgId ?? "all"}";
            if (_feedCache.TryGetValue(cacheKey, out var cached) && (DateTime.UtcNow - cached.CachedAt).TotalMinutes < 15)
            {
                return Results.Content(cached.Content, "application/xml", Encoding.UTF8);
            }

            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            if (context.Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "https://socratic.uz";
            }

            var orgService = clientFactory.GetClient<Organization>();
            var prodService = clientFactory.GetClient<Product>();

            Guid? targetOrgGuid = null;
            if (Guid.TryParse(cleanOrgId, out var parsedGuid))
            {
                targetOrgGuid = parsedGuid;
            }

            var orgs = new List<Organization>();
            if (targetOrgGuid.HasValue)
            {
                try
                {
                    var singleOrgCall = orgService.Read(new Organization { Id = targetOrgGuid.Value });
                    if (await singleOrgCall.ResponseStream.MoveNext(cancellationToken) && singleOrgCall.ResponseStream.Current?.Id != null)
                    {
                        orgs.Add(singleOrgCall.ResponseStream.Current);
                    }
                }
                catch { }

                if (orgs.Count == 0)
                {
                    orgs.Add(new Organization { Id = targetOrgGuid.Value, Name = "Организация" });
                }
            }
            else
            {
                try
                {
                    var orgCall = orgService.Read(new Organization());
                    while (await orgCall.ResponseStream.MoveNext(cancellationToken))
                    {
                        if (orgCall.ResponseStream.Current?.Id != null)
                        {
                            orgs.Add(orgCall.ResponseStream.Current);
                        }
                    }
                }
                catch { }

                if (orgs.Count == 0)
                {
                    orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.DemoId, Name = "Demo" });
                    orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.SocraticId, Name = "Socratic" });
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine($"<yml_catalog date=\"{DateTime.UtcNow:yyyy-MM-dd HH:mm}\">");

            var mainOrg = orgs.FirstOrDefault();
            var shopName = orgs.Count == 1 ? $"Socratic - {mainOrg?.Name}" : "Socratic Marketplace";
            var shopCompany = orgs.Count == 1 ? (mainOrg?.Name ?? "Socratic") : "Socratic Ecosystem";
            var shopUrl = targetOrgGuid.HasValue ? $"{baseUrl}/kiosk/{targetOrgGuid.Value}" : baseUrl;

            sb.AppendLine("  <shop>");
            sb.AppendLine($"    <name>{EscapeXml(shopName)}</name>");
            sb.AppendLine($"    <company>{EscapeXml(shopCompany)}</company>");
            sb.AppendLine($"    <url>{shopUrl}</url>");
            sb.AppendLine("    <currencies>");
            sb.AppendLine("      <currency id=\"UZS\" rate=\"1\"/>");
            sb.AppendLine("    </currencies>");
            sb.AppendLine("    <categories>");
            sb.AppendLine("      <category id=\"1\">Каталог товаров</category>");
            sb.AppendLine("    </categories>");
            sb.AppendLine("    <offers>");

            foreach (var org in orgs)
            {
                var prodCount = 0;
                try
                {
                    var prodCall = prodService.Read(new Product { OrganizationId = org.Id });
                    while (await prodCall.ResponseStream.MoveNext(cancellationToken))
                    {
                        var prod = prodCall.ResponseStream.Current;
                        if (prod?.Id != null && !string.IsNullOrWhiteSpace(prod.Name))
                        {
                            prodCount++;
                            var slug = SlugHelper.GenerateSlug(prod.Name);
                            var prodUrl = $"{baseUrl}/kiosk/{org.Id}/product/{prod.Id}/{slug}";
                            var img = prod.Images?.FirstOrDefault()?.Url;
                            if (string.IsNullOrWhiteSpace(img)) img = $"{baseUrl}/img/icon-512.png";
                            var desc = !string.IsNullOrWhiteSpace(prod.Description) ? prod.Description : prod.Name;
                            var price = (long)Math.Round(prod.Price);

                            sb.AppendLine($"      <offer id=\"{prod.Id}\" available=\"true\">");
                            sb.AppendLine($"        <name>{EscapeXml(prod.Name)}</name>");
                            sb.AppendLine($"        <url>{prodUrl}</url>");
                            sb.AppendLine($"        <price>{price}</price>");
                            sb.AppendLine("        <currencyId>UZS</currencyId>");
                            sb.AppendLine("        <categoryId>1</categoryId>");
                            sb.AppendLine($"        <picture>{EscapeXml(img)}</picture>");
                            sb.AppendLine($"        <description>{EscapeXml(desc)}</description>");
                            sb.AppendLine($"        <vendor>{EscapeXml(org.Name)}</vendor>");
                            sb.AppendLine("      </offer>");
                        }
                    }
                }
                catch { }

                if (prodCount == 0)
                {
                    void AddFallbackOffer(string id, string name, decimal price, string desc)
                    {
                        var slug = SlugHelper.GenerateSlug(name);
                        var prodUrl = $"{baseUrl}/kiosk/{org.Id}/product/{id}/{slug}";
                        var img = $"{baseUrl}/img/icon-512.png";
                        sb.AppendLine($"      <offer id=\"{id}\" available=\"true\">");
                        sb.AppendLine($"        <name>{EscapeXml(name)}</name>");
                        sb.AppendLine($"        <url>{prodUrl}</url>");
                        sb.AppendLine($"        <price>{(long)price}</price>");
                        sb.AppendLine("        <currencyId>UZS</currencyId>");
                        sb.AppendLine("        <categoryId>1</categoryId>");
                        sb.AppendLine($"        <picture>{EscapeXml(img)}</picture>");
                        sb.AppendLine($"        <description>{EscapeXml(desc)}</description>");
                        sb.AppendLine($"        <vendor>{EscapeXml(org.Name)}</vendor>");
                        sb.AppendLine("      </offer>");
                    }

                    if (org.Id == SharedKernel.Extensions.ObjectIdExtension.DemoId)
                    {
                        AddFallbackOffer("b1a10001-0000-0000-0000-000000000001", "Узбекский Плов Ташкент", 45000, "Традиционный праздничный ташкентский плов на хлопковом масле с желтой морковью и казы.");
                        AddFallbackOffer("b1a10001-0000-0000-0000-000000000002", "Персональный Конфигуратор Пиццы", 65000, "Свежая пицца с индивидуальным подбором топпингов и сырного борта.");
                        AddFallbackOffer("b1a10001-0000-0000-0000-000000000003", "VIP Билет на Концерт", 250000, "Электронный билет в первый сектор с доступом в лаунж-зону.");
                    }
                    else if (org.Id == SharedKernel.Extensions.ObjectIdExtension.SocraticId)
                    {
                        AddFallbackOffer("b1a10002-0000-0000-0000-000000000001", "Тариф Бизнес Про Сети и Ритейл", 500000, "Подписка на облачный бэкофис, аналитику и мониторинг точек.");
                        AddFallbackOffer("b1a10002-0000-0000-0000-000000000002", "Заявка на подключение и регистрацию организации", 100000, "Быстрый старт автоматизации торговой точки под ключ.");
                        AddFallbackOffer("b1a10002-0000-0000-0000-000000000003", "Комплект POS терминал и Киоск самообслуживания", 8500000, "Готовое аппаратное решение с термопринтером и сканером QR.");
                    }
                }
            }

            sb.AppendLine("    </offers>");
            sb.AppendLine("  </shop>");
            sb.AppendLine("</yml_catalog>");

            var resultXml = sb.ToString();
            _feedCache[cacheKey] = (resultXml, DateTime.UtcNow);
            return Results.Content(resultXml, "application/xml", Encoding.UTF8);
        }).DisableAntiforgery();

        app.MapGet("/feeds/google/{orgId?}", async (
            string? orgId,
            IServiceClientFactory clientFactory,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var cleanOrgId = orgId?.Trim();
            if (!string.IsNullOrEmpty(cleanOrgId) && cleanOrgId.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                cleanOrgId = cleanOrgId[..^4];
            }

            var cacheKey = $"google_{cleanOrgId ?? "all"}";
            if (_feedCache.TryGetValue(cacheKey, out var cached) && (DateTime.UtcNow - cached.CachedAt).TotalMinutes < 15)
            {
                return Results.Content(cached.Content, "application/xml", Encoding.UTF8);
            }

            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            if (context.Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "https://socratic.uz";
            }

            var orgService = clientFactory.GetClient<Organization>();
            var prodService = clientFactory.GetClient<Product>();

            Guid? targetOrgGuid = null;
            if (Guid.TryParse(cleanOrgId, out var parsedGuid))
            {
                targetOrgGuid = parsedGuid;
            }

            var orgs = new List<Organization>();
            if (targetOrgGuid.HasValue)
            {
                try
                {
                    var singleOrgCall = orgService.Read(new Organization { Id = targetOrgGuid.Value });
                    if (await singleOrgCall.ResponseStream.MoveNext(cancellationToken) && singleOrgCall.ResponseStream.Current?.Id != null)
                    {
                        orgs.Add(singleOrgCall.ResponseStream.Current);
                    }
                }
                catch { }

                if (orgs.Count == 0)
                {
                    orgs.Add(new Organization { Id = targetOrgGuid.Value, Name = "Организация" });
                }
            }
            else
            {
                try
                {
                    var orgCall = orgService.Read(new Organization());
                    while (await orgCall.ResponseStream.MoveNext(cancellationToken))
                    {
                        if (orgCall.ResponseStream.Current?.Id != null)
                        {
                            orgs.Add(orgCall.ResponseStream.Current);
                        }
                    }
                }
                catch { }

                if (orgs.Count == 0)
                {
                    orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.DemoId, Name = "Demo" });
                    orgs.Add(new Organization { Id = SharedKernel.Extensions.ObjectIdExtension.SocraticId, Name = "Socratic" });
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<rss version=\"2.0\" xmlns:g=\"http://base.google.com/ns/1.0\">");

            var mainOrg = orgs.FirstOrDefault();
            var title = orgs.Count == 1 ? $"Socratic - {mainOrg?.Name}" : "Socratic Catalog";
            var desc = "Товары и услуги экосистемы Socratic";
            var link = targetOrgGuid.HasValue ? $"{baseUrl}/kiosk/{targetOrgGuid.Value}" : baseUrl;

            sb.AppendLine("  <channel>");
            sb.AppendLine($"    <title>{EscapeXml(title)}</title>");
            sb.AppendLine($"    <link>{link}</link>");
            sb.AppendLine($"    <description>{EscapeXml(desc)}</description>");

            foreach (var org in orgs)
            {
                var prodCount = 0;
                try
                {
                    var prodCall = prodService.Read(new Product { OrganizationId = org.Id });
                    while (await prodCall.ResponseStream.MoveNext(cancellationToken))
                    {
                        var prod = prodCall.ResponseStream.Current;
                        if (prod?.Id != null && !string.IsNullOrWhiteSpace(prod.Name))
                        {
                            prodCount++;
                            var slug = SlugHelper.GenerateSlug(prod.Name);
                            var prodUrl = $"{baseUrl}/kiosk/{org.Id}/product/{prod.Id}/{slug}";
                            var img = prod.Images?.FirstOrDefault()?.Url;
                            if (string.IsNullOrWhiteSpace(img)) img = $"{baseUrl}/img/icon-512.png";
                            var itemDesc = !string.IsNullOrWhiteSpace(prod.Description) ? prod.Description : prod.Name;
                            var price = (long)Math.Round(prod.Price);

                            sb.AppendLine("    <item>");
                            sb.AppendLine($"      <g:id>{prod.Id}</g:id>");
                            sb.AppendLine($"      <g:title>{EscapeXml(prod.Name)}</g:title>");
                            sb.AppendLine($"      <g:description>{EscapeXml(itemDesc)}</g:description>");
                            sb.AppendLine($"      <g:link>{prodUrl}</g:link>");
                            sb.AppendLine($"      <g:image_link>{EscapeXml(img)}</g:image_link>");
                            sb.AppendLine("      <g:condition>new</g:condition>");
                            sb.AppendLine("      <g:availability>in_stock</g:availability>");
                            sb.AppendLine($"      <g:price>{price} UZS</g:price>");
                            sb.AppendLine($"      <g:brand>{EscapeXml(org.Name)}</g:brand>");
                            sb.AppendLine("    </item>");
                        }
                    }
                }
                catch { }

                if (prodCount == 0)
                {
                    void AddFallbackItem(string id, string name, decimal price, string fallbackDesc)
                    {
                        var slug = SlugHelper.GenerateSlug(name);
                        var prodUrl = $"{baseUrl}/kiosk/{org.Id}/product/{id}/{slug}";
                        var img = $"{baseUrl}/img/icon-512.png";
                        sb.AppendLine("    <item>");
                        sb.AppendLine($"      <g:id>{id}</g:id>");
                        sb.AppendLine($"      <g:title>{EscapeXml(name)}</g:title>");
                        sb.AppendLine($"      <g:description>{EscapeXml(fallbackDesc)}</g:description>");
                        sb.AppendLine($"      <g:link>{prodUrl}</g:link>");
                        sb.AppendLine($"      <g:image_link>{EscapeXml(img)}</g:image_link>");
                        sb.AppendLine("      <g:condition>new</g:condition>");
                        sb.AppendLine("      <g:availability>in_stock</g:availability>");
                        sb.AppendLine($"      <g:price>{(long)price} UZS</g:price>");
                        sb.AppendLine($"      <g:brand>{EscapeXml(org.Name)}</g:brand>");
                        sb.AppendLine("    </item>");
                    }

                    if (org.Id == SharedKernel.Extensions.ObjectIdExtension.DemoId)
                    {
                        AddFallbackItem("b1a10001-0000-0000-0000-000000000001", "Узбекский Плов Ташкент", 45000, "Традиционный праздничный ташкентский плов на хлопковом масле с желтой морковью и казы.");
                        AddFallbackItem("b1a10001-0000-0000-0000-000000000002", "Персональный Конфигуратор Пиццы", 65000, "Свежая пицца с индивидуальным подбором топпингов и сырного борта.");
                        AddFallbackItem("b1a10001-0000-0000-0000-000000000003", "VIP Билет на Концерт", 250000, "Электронный билет в первый сектор с доступом в лаунж-зону.");
                    }
                    else if (org.Id == SharedKernel.Extensions.ObjectIdExtension.SocraticId)
                    {
                        AddFallbackItem("b1a10002-0000-0000-0000-000000000001", "Тариф Бизнес Про Сети и Ритейл", 500000, "Подписка на облачный бэкофис, аналитику и мониторинг точек.");
                        AddFallbackItem("b1a10002-0000-0000-0000-000000000002", "Заявка на подключение и регистрацию организации", 100000, "Быстрый старт автоматизации торговой точки под ключ.");
                        AddFallbackItem("b1a10002-0000-0000-0000-000000000003", "Комплект POS терминал и Киоск самообслуживания", 8500000, "Готовое аппаратное решение с термопринтером и сканером QR.");
                    }
                }
            }

            sb.AppendLine("  </channel>");
            sb.AppendLine("</rss>");

            var resultXml = sb.ToString();
            _feedCache[cacheKey] = (resultXml, DateTime.UtcNow);
            return Results.Content(resultXml, "application/xml", Encoding.UTF8);
        }).DisableAntiforgery();
    }
}
