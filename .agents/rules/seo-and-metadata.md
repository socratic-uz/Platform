# Socratic SEO & Metadata Standards

This document specifies standards for search engine optimization (Google, Yandex), social graph previews (OpenGraph, Twitter, Instagram), structured data, and advertising integration in Socratic.

---

## 1. No Global Hardcoded Page Titles/Descriptions

- Do NOT place static page titles, descriptions, and keywords in `App.razor`.
- Every routed page component (`@page "/path"` or `[Route("/path")]` in code-behind) MUST provide its own title and metadata using `<PageTitle>` and `<HeadContent>`.
- Alternatively, use the standardized `SeoHead.razor` (`src/Frontend/Platform/Shared/DesignSystem/Layout/Components/SeoHead.razor`) component.

---

## 2. Multilingual SEO & `hreflang`

Because Socratic is tri-lingual (`uz`, `ru`, `en`):
1. The `<html>` tag in `App.razor` must dynamically reflect the active culture:
   ```razor
   <html lang="@CultureInfo.CurrentUICulture.TwoLetterISOLanguageName">
   ```
2. Public catalog pages, organizations, and landings should declare alternate language links:
   ```html
   <link rel="alternate" hreflang="uz" href="https://socratic.uz/uz/..." />
   <link rel="alternate" hreflang="ru" href="https://socratic.uz/ru/..." />
   <link rel="alternate" hreflang="en" href="https://socratic.uz/en/..." />
   <link rel="alternate" hreflang="x-default" href="https://socratic.uz/..." />
   ```
3. Always include a `<link rel="canonical" href="..." />` to avoid duplicate content penalties.

---

## 3. Structured Data (Schema.org JSON-LD)

For public pages across the 28 universal commercial modes, inject JSON-LD structured data into `<HeadContent>`:
- **Food & Beverage / Restaurant**: `@type: "Restaurant"`, menus, opening hours.
- **Retail / Products**: `@type: "Product"`, `offers`, `price`, `priceCurrency: "UZS"`.
- **Places / Venues**: `@type: "LocalBusiness"`, `geo`, `address`.
- **Breadcrumbs**: `@type: "BreadcrumbList"` for hierarchical catalog navigation.
- **Ratings & Reviews**: `@type: "AggregateRating"` for Google rich snippets.

---

## 4. Crawling & Indexing Boundary

- Socratic serves dynamic `robots.txt` and `sitemap.xml` via ASP.NET Core endpoints (`SeoEndpoints.cs`).
- Terminal, Cashier, and Operational routes (internal POS operational paths) MUST be excluded from indexing:
   ```html
   <meta name="robots" content="noindex, nofollow" />
   ```
- `robots.txt` must explicitly `Disallow:` non-canonical operational routes (`/terminal`, `/orders`, `/store/`, `/api/`, `/hubs/`).

---

## 5. Crawl Graph & Semantic Navigation (`<a href>` vs `@onclick`)

> [!CRITICAL]
> **Googlebot / YandexBot Discovery Law**:
> Search engine crawlers only discover and crawl URLs through standard HTML5 `<a href="...">` links. Crawlers do NOT execute Blazor C# `@onclick` lambdas, nor do they reliably pierce Web Component shadow boundaries for custom button links.

1. **Public Catalog Cards**:
   All entity cards in public catalogs and showcases ([`Kiosk.razor`](file:///src/Frontend/Retail/PointOfSale/Pages/Kiosk.razor), landing, search) MUST provide clean, crawlable `<a href="...">` links to the canonical product URL:
   ```razor
   <h3>
       <a href=@productUrl aria-label=@GetProductDetailsAria(context.Name)>
           @context.Name
       </a>
   </h3>
   ```
2. **Clear Separation of Navigation vs Actions**:
   - The card title `<h3><a href=@productUrl>` is a pure HTML5 navigation link that leads directly to the dedicated entity page (`ProductPage.razor`), ensuring a natural Crawl Graph for Googlebot and standard e-commerce navigation for users.
   - Quick experiences, instant checkout, and modal configurators are triggered exclusively by dedicated action buttons in the card footer (`<footer><Button OnClick=@OpenExperience>`).

---

## 6. Soft-404 Prevention & Degradation Strategy

1. **Entity Not Found Handling**:
   When an entity (Product, Organization, Event) is not found in the database during SSR prerendering:
   - Emitting HTTP 200 with an empty skeleton or generic "Not Found" UI is treated by Google as a **Soft 404 defect**.
   - The component MUST inject a `<meta name="robots" content="noindex, nofollow" />` tag into `<HeadContent>`:
     ```razor
     @if (product == null && !IsLoading)
     {
         <PageTitle>@L[Store_NotFoundTitle]</PageTitle>
         <HeadContent>
             <meta name="robots" content="noindex, nofollow" />
         </HeadContent>
     }
     ```
2. **Graceful Fallbacks**:
   Do NOT render synthetic dummy products ("Demo Item") for real production tenant routes, as Google will index fake placeholder data.

---

## 7. Sitemap Scalability & Crawl Budget

1. **Sitemap Indexing**:
   - High-volume catalogs must use partitioned sitemaps: `/sitemap.xml` for general discovery, and `/sitemap/{orgId}.xml` for tenant-specific product trees.
   - All generated URLs must be cached in memory (`_feedCache`) with background invalidation to guarantee response times $< 500\text{ms}$ under crawler loads.
2. **Crawl Budget Conservation**:
   - Disallow legacy alias paths (`/store/`) in `robots.txt` so Googlebot concentrates crawl budget strictly on canonical `/kiosk/{orgId}/product/{id}/{slug}` URLs.

---

## 8. Advertising & Monetization Integration (`<aside ads="...">`)

1. **Semantic Isolation**:
   All third-party ad networks (Google AdSense, Yandex RTB, partner banners) must be wrapped in an `<aside ads="...">` tag with an explicit accessible label:
   ```razor
   <aside ads="google" aria-label="Advertisement">
       <ins class="adsbygoogle" ...></ins>
   </aside>
   ```
   This guarantees that search engine algorithms do not index sponsored text as organic site content.
2. **Leaf Container Principle**:
   Ad scripts inject dynamic iframes. Ad unit containers must be empty leaf nodes without child Blazor DOM diffing.
3. **Cumulative Layout Shift (CLS = 0)**:
   Ad units must reserve explicit dimensions in CSS Grid / Container Queries (`aspect-ratio`, `min-height`) to prevent layout shifts and protect Google Core Web Vitals rankings.
