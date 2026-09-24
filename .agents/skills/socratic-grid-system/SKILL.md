---
name: socratic-grid-system
description: Strict CSS Grid and Subgrid engineering architecture for Socratic Blazor applications. USE FOR: building all page shells, widgets, card grids, split POS layouts, toolbars, and form rows exclusively with CSS Grid (display: grid). DO NOT USE FOR: Flexbox (display: flex), which is strictly forbidden across Socratic.
---

# Socratic Design Engineering: Strict CSS Grid & Subgrid Systems

In Socratic, **Flexbox (`display: flex`) is strictly prohibited** for custom components. All alignment, rows, columns, toolbars, cards, and page shells must be implemented exclusively with **CSS Grid (`display: grid`)**.

---

## 1. 🧱 Fundamental Grid Building Blocks

Instead of flex rows and columns, use standard Grid stacks:

### 1. Column Stack (Vertical List):
```css
.stack-col {
    display: grid;
    grid-auto-flow: row;
    gap: 16px;
}
```

### 2. Row Stack (Horizontal Toolbar):
```css
.stack-row {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: max-content;
    align-items: center;
    gap: 12px;
}
```

### 3. Space-Between Row (Header / Footer):
```css
.grid-row-between {
    display: grid;
    grid-template-columns: 1fr auto;
    align-items: center;
    gap: 16px;
    width: 100%;
}
```

---

## 2. 🎛️ Responsive Auto-Fit Cards (Fluid Grid)

Do **NOT** write fragile media queries (`@media (max-width: 768px)`) for card grids. Use intrinsic fluid grid columns:

```css
.product-card-grid {
    display: grid;
    /* Adapts seamlessly from 1 column on mobile to 6 columns on ultra-wide POS */
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 240px), 1fr));
    gap: 16px;
}
```

> **CRITICAL TIP (`minmax(0, 1fr)`)**:
> Always use `minmax(0, 1fr)` instead of plain `1fr` when a grid column contains text, tables, or code that might truncate or overflow. In CSS Grid, `1fr` has a default `min-width: auto`, which causes grid blowouts. `minmax(0, 1fr)` enforces true containment.

---

## 3. 🎯 CSS Subgrid for Aligned Card Elements

When product cards have varying title lengths, images, or descriptions, card footers and prices often become misaligned. In Socratic, use `grid-template-rows: subgrid` to keep all elements in sync across rows:

```css
/* Parent Card Grid */
.catalog-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    grid-auto-rows: auto auto 1fr auto; /* [Image] [Title] [Body] [Footer] */
    gap: 16px;
}

/* Individual Card (Spans all 4 row tracks and inherits row sizing) */
.catalog-card {
    display: grid;
    grid-row: span 4;
    grid-template-rows: subgrid;
    border-radius: var(--md-sys-shape-expressive-card);
    background: var(--md-sys-color-surface-container);
    padding: 16px;
}
```

---

## 4. 🛒 POS & Kiosk Split-Screen Register Layout

The flagship split layout for cashiers and ordering kiosks:

```css
.pos-layout {
    display: grid;
    /* Left catalog takes available space; right cart sidebar is bounded */
    grid-template-columns: minmax(0, 1fr) clamp(340px, 30vw, 440px);
    gap: 20px;
    height: calc(100vh - 120px);
    align-items: stretch;
}

/* Right Cart/Ticket Panel (3 distinct grid areas) */
.pos-sidebar {
    display: grid;
    grid-template-rows: auto 1fr auto; /* [Header] [Scrollable Items] [Actions/Pay] */
    background: var(--md-sys-color-surface-container-lowest);
    border-radius: var(--md-sys-shape-expressive-card);
    border: 1px solid var(--md-sys-color-outline-variant);
    padding: 20px;
    overflow: hidden;
}

.pos-cart-items {
    overflow-y: auto;
    overscroll-behavior-y: contain;
}

.pos-cart-footer {
    display: grid;
    grid-template-rows: auto auto;
    gap: 12px;
    border-top: 1px solid var(--md-sys-color-outline-variant);
    padding-top: 16px;
}
```

---

## 5. 🚫 Common Flexbox Anti-Patterns & Grid Fixes

| Flexbox Anti-Pattern (FORBIDDEN) | Strict CSS Grid Solution (REQUIRED) |
| :--- | :--- |
| `display: flex; flex-direction: column; gap: 8px;` | `display: grid; grid-auto-flow: row; gap: 8px;` |
| `display: flex; align-items: center; gap: 12px;` | `display: grid; grid-auto-flow: column; grid-auto-columns: max-content; align-items: center; gap: 12px;` |
| `display: flex; justify-content: space-between;` | `display: grid; grid-template-columns: 1fr auto; align-items: center;` |
| `display: flex; flex-wrap: wrap; gap: 16px;` | `display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px;` |
| `display: flex; justify-content: center; align-items: center;` | `display: grid; place-content: center; justify-items: center;` |

---

## 6. 🔍 Validation

Run `Audit-BlazorUiQuality.ps1` to detect and eliminate any forbidden `display: flex` declarations across `.razor` and `.razor.css` files.
