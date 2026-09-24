---
name: socratic-frontend
description: Comprehensive frontend and design system guide for Socratic Blazor applications (Platform/Shared, Web.UI, Terminal, Kiosk). USE FOR: authoring Blazor components with Google Material Design 3 Web Components (@material/web), isolated Vanilla CSS without Tailwind, Smart.Web custom controls, Markdown.Web rendering, eliminating UI flicker/FOUC during server prerendering/WASM hydration, font ligature loading, and CSS layout architecture. DO NOT USE FOR: TailwindCSS utility classes or backend database queries.
---

# Socratic Frontend Architecture: Material Web 3, Isolated CSS & Anti-Flicker

The Socratic user interface is built on **Blazor Hybrid/Web App** (`Platform/Shared` component library, `Web.UI` host) powered by **Google Material Design 3 Web Components** (`@material/web`).

---

## 1. 🎨 Material Design 3 Web Components

Socratic strictly avoids third-party Blazor UI wrappers (e.g. MudBlazor, AntDesign) and uses official Material Design 3 custom elements.

### Essential Components:
- Buttons: `<md-filled-button>`, `<md-outlined-button>`, `<md-text-button>`, `<md-elevated-button>`
- Text Fields: `<md-outlined-text-field label="..." value="@val">`, `<md-filled-text-field>`
- Icons: `<md-icon>shopping_cart</md-icon>` (ligature icons via Material Symbols Outlined)
- Dialogs: `<md-dialog open="@isOpen">`
- Select & Menus: `<md-outlined-select>`, `<md-select-option>`
- Switches & Checkboxes: `<md-switch selected="@isActive">`, `<md-checkbox>`
- Chips: `<md-chip-set>`, `<md-assist-chip label="...">`, `<md-filter-chip>`
- Tabs: `<md-tabs>`, `<md-primary-tab>`, `<md-secondary-tab>`

### Material 3 CSS Tokens:
Always use design tokens instead of hardcoded hex colors:
- Background / Surface: `var(--md-sys-color-surface)`, `var(--md-sys-color-surface-container)`
- Primary: `var(--md-sys-color-primary)`, `var(--md-sys-color-on-primary)`
- Secondary / Tertiary: `var(--md-sys-color-secondary)`, `var(--md-sys-color-tertiary)`
- Outlines & Borders: `var(--md-sys-color-outline)`, `var(--md-sys-color-outline-variant)`

---

## 2. 🚫 Strict Layout & Styling Rules (NO TailwindCSS, NO Flexbox)

> **CRITICAL RULES**:
> 1. Do **NOT** use Tailwind utility classes (`flex items-center justify-between p-4 bg-gray-900 text-white`, etc.).
> 2. Do **NOT** use Flexbox (`display: flex`). All component layouts, stacks, and rows must use **CSS Grid (`display: grid`)**.
> 3. All components must use isolated `.razor.css` files with semantic CSS class names and Material 3 CSS variables (`var(--md-sys-color-*)`).

### Example (CSS Grid Layout):
```razor
<!-- ProductCard.razor -->
<div class="product-card">
    <div class="product-header">
        <h3 class="product-title">@Product.Name</h3>
        <span class="product-price">@Product.Price.ToString("C")</span>
    </div>
    <md-filled-button @onclick="AddToCart">Add to Cart</md-filled-button>
</div>
```

```css
/* ProductCard.razor.css */
.product-card {
    display: grid;
    padding: 16px;
    border-radius: 16px;
    background-color: var(--md-sys-color-surface-container);
    border: 1px solid var(--md-sys-color-outline-variant);
    gap: 12px;
}

.product-header {
    display: grid;
    grid-template-columns: 1fr auto;
    align-items: center;
    gap: 8px;
}

.product-title {
    color: var(--md-sys-color-on-surface);
    font-size: 1.125rem;
    font-weight: 600;
}

.product-price {
    color: var(--md-sys-color-primary);
    font-weight: 700;
}
```

---


## 3. ⚡ Zero-Flicker (Anti-FOUC) & SSR Prerendering Hydration

Blazor Web Apps prerender on the server before client-side WebAssembly/SignalR activates. To eliminate flashing and layout shifts (CLS):

### 1. PersistentComponentState (Prevent Double Network Fetch)
```csharp
@inject PersistentComponentState ApplicationState
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    persistSubscription = ApplicationState.RegisterOnPersisting(PersistData);
    if (!ApplicationState.TryTakeFromJson<List<ProductDto>>("products_cache", out var cached))
    {
        products = await ProductClient.GetProductsAsync();
    }
    else
    {
        products = cached;
    }
}
```

### 2. Zero-Flicker Theme Detection in `<head>`
Never read themes inside Blazor's async lifecycle. Execute synchronous inline JS in `App.razor`:
```html
<script>
    (function() {
        var theme = localStorage.getItem('socratic_theme') || 
            (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        document.documentElement.setAttribute('data-theme', theme);
    })();
</script>
```

### 3. Web Components `:not(:defined)` Masking
Prevent unstyled flash while `@material/web` scripts load:
```css
md-filled-button:not(:defined),
md-outlined-text-field:not(:defined),
md-dialog:not(:defined) {
    opacity: 0;
    pointer-events: none;
}
md-filled-button:defined,
md-outlined-text-field:defined {
    opacity: 1;
    transition: opacity 0.15s ease-in;
}
```

### 4. Icon Ligature FOUT Protection
Prevent text like `"search"` or `"close"` appearing before the font loads:
- Use `font-display: block;` in `@font-face`.
- Preload the WOFF2 font in `<head>`.
- Set `md-icon:not(:defined) { color: transparent !important; font-size: 0 !important; }`.

---

## 4. 🧩 Smart Web & Markdown Web Custom Components

- **Smart.Web**: Custom components for interactive grids, canvas drawing, and high-density data tables.
- **Markdown.Web**: Lightweight, safe Markdown rendering for documentation and release notes without arbitrary raw HTML injection.
