# Socratic SEO & Metadata Standards

This document specifies standards for search engine optimization (Google, Yandex), social graph previews (OpenGraph, Twitter, Instagram), and structured data in Socratic.

---

## 1. No Global Hardcoded Page Titles/Descriptions

- Do NOT place static page titles, descriptions, and keywords in `App.razor`.
- Every routed page component (`@page "/path"`) MUST provide its own title and metadata using `<PageTitle>` and `<HeadContent>`.
- Alternatively, use the standardized [SeoHead.razor](file:///c:/Users/owner/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Layout/Components/SeoHead.razor) component.

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

---

## 4. Crawling & Indexing

- Socratic serves dynamic `robots.txt` and `sitemap.xml` via ASP.NET Core endpoints.
- Terminal, Kiosk, and Cashier routes (internal POS operational paths) MUST include:
  ```html
  <meta name="robots" content="noindex, nofollow" />
  ```
