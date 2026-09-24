---
name: socratic-standards
description: Cross-cutting architectural standards for Socratic. USE FOR: managing Material Design 3 themes (dark/light/system) and dynamic brand accent palettes; implementing multi-language localization (ru, uz, en) and 3-way .resx resource synchronization; optimizing SEO, OpenGraph metadata, and JSON-LD structured data; authoring unit and integration tests using xUnit, mocking Cassandra.ISession, gRPC clients, and ScyllaFixture. DO NOT USE FOR: writing ScyllaDB production migrations.
---

# Socratic Cross-Cutting Standards: Theming, i18n, SEO & Testing

This guide unifies the global quality standards required across all Socratic microservices and Blazor UI components.

---

## 1. 🌈 Material Design 3 Theming & Dynamic Accents

Theme tokens reside in `src/Frontend/Platform/Shared/DesignSystem/Layout/wwwroot/css/material-theme.css/`:
- `dark.css`: Scoped to `:root[data-theme="dark"]`
- `light.css`: Scoped to `:root[data-theme="light"]`
- `Color.razor` & `Color.razor.js`: Injects computed M3 tonal palettes when an organization customizes its brand accent color.

### Core Rules:
1. Always scope CSS with `:root[data-theme="dark"]` or `:root[data-theme="light"]`.
2. Ensure `App.razor` has the synchronous `<script>` in `<head>` to prevent white FOUC flash.
3. In components, always use `--md-sys-color-*` variables instead of hardcoded colors.

---

## 2. 🌐 Multi-Language Localization (RU, UZ, EN)

Socratic officially supports three locales:
- Russian (`ru-RU`) — default.
- Uzbek Latin (`uz-Latn-UZ`).
- English (`en-US`).

### Resource Files:
All user-facing strings are defined symmetrically in `src/Frontend/Platform/Shared/DesignSystem/Layout/Resources/`:
- `ResourceRu.resx`
- `ResourceUz.resx`
- `ResourceEn.resx`

Every newly introduced key **must** exist across all three files.

### Culture Switching in Blazor InteractiveAuto:
When the user selects a language, navigate to `/api/culture/set?culture={culture}&redirectUri={uri}` so the server writes the `.AspNetCore.Culture` cookie and re-renders both SSR and client WASM in total sync.

---

## 3. 🔍 SEO, OpenGraph & JSON-LD Structured Data

Public customer-facing pages (Landing, Catalog, Places, Events) must include metadata for search engines and social sharing:
- Unique `<title>` and `<meta name="description">` per page.
- OpenGraph tags: `og:title`, `og:description`, `og:image`, `og:url`.
- Structured JSON-LD schema (e.g. `Schema.org/Restaurant`, `Schema.org/Product`, `Schema.org/Place`).

---

## 4. 🧪 Testing Standards (No EF Core, xUnit & Mocking)

Socratic tests reside in `*.Test` projects and avoid relational database dependencies.

### Running Tests:
```bash
# Run unit tests across all microservices
dotnet test Socratic.slnx --filter "Category!=Integration"

# Run tests for a specific microservice
dotnet test src/Backend/Ordering/Ordering.Test/Ordering.Test.csproj
```

### Mocking gRPC Client & IServiceClient<T>:
```csharp
var clientMock = new Mock<IServiceClient<ProductDto>>();
clientMock.Setup(c => c.Read(It.IsAny<ProductDto>()))
    .ReturnsAsync(new List<ProductDto>
    {
        new ProductDto { Id = Guid.NewGuid().ToString(), Name = "Burger", Price = 45000 }
    });
```

### Integration Tests with ScyllaFixture:
```csharp
[Collection("ScyllaCollection")]
public class ScyllaLedgerIntegrationTests : IClassFixture<ScyllaFixture>
{
    private readonly ScyllaFixture _fixture;
    public ScyllaLedgerIntegrationTests(ScyllaFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task InsertAndRetrieveTransaction_ReturnsCorrectBalance()
    {
        var session = _fixture.Session;
        var repo = new ScyllaLedgerRepository(session);
        // Execute real CQL and verify shard distribution
    }
}
```
