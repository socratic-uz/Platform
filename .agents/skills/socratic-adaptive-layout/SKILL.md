---
name: socratic-adaptive-layout
description: Official Material Design 3 (M3) Adaptive Window Size Classes & Cross-Platform Layout standard for Socratic Blazor applications. USE FOR: responsive layouts across Mobile POS (<600px Compact), Waiter Tablets (600-839px Medium), Desktop/Fixed POS (840-1199px Expanded & 1200-1599px Large), Self-Service Kiosks (portrait 1080x1920 with ADA vertical reachability), and Ultrawide displays (>=1600px Extra-Large). Implements canonical M3 Pane patterns (List-Detail, Supporting-Pane, Kiosk Matrix) exclusively using strict CSS Grid. DO NOT USE FOR: ad-hoc non-standard breakpoints or Flexbox.
license: MIT
---

# Socratic Design Engineering: M3 Adaptive Window Size Classes & Cross-Platform Layouts

## 1. 📐 Google Material Design 3 Window Size Classes

Socratic applications run across diverse hardware: handheld smart POS terminals (PAX, Sunmi), waiter tablets, countertop POS terminals, vertical self-service kiosks, and ultrawide desktop monitors.

To ensure consistency, resilience, and zero layout breakage across platforms, Socratic strictly adopts the **Google Material Design 3 Breakpoint Classes**:

| Window Class | Viewport Width | Typical Socratic Hardware | Canonical Layout Structure |
| :--- | :--- | :--- | :--- |
| **Compact** | `< 600px` | Handheld POS, smartphones, mobile scanners | Single Pane (Vertical Stack), Bottom Navigation, Full-bleed Modal Sheets, Touch $\ge 48\text{px}$ |
| **Medium** | `600px – 839px` | Foldables (unfolded), 8"-10" waiter tablets (portrait) | Navigation Rail (80px) + Main Pane + Collapsible Bottom Sheet |
| **Expanded** | `840px – 1199px` | 10"-12" counter POS, tablets (landscape) | **Supporting Pane (2 Panes)**: Catalog Matrix (1fr) + Fixed Order Cart (340–400px) |
| **Large** | `1200px – 1599px` | 15.6" Desktop POS, Self-Checkout Kiosks | **2 or 3 Panes**: Categories + Product Grid + Docked Order Summary, Touch $\ge 56\text{px}$ |
| **Extra-Large** | `$\ge 1600px$` | 24"-32" Back-office Desktop, 4K Signage | Multi-pane / Clamped Content Container (`max-width: 1600px; margin: 0 auto;`) |

---

## 2. 🛡️ Media Query Standard

Never use arbitrary ad-hoc pixel widths (e.g. `768px`, `992px`, `1024px`). Always use the canonical M3 breakpoints:

```css
/* ─── Compact (< 600px) ──────────────────────────────────────────────────────── */
@media (max-width: 599.98px) {
    /* Mobile POS & Phone adjustments */
}

/* ─── Medium (600px – 839.98px) ──────────────────────────────────────────────── */
@media (min-width: 600px) and (max-width: 839.98px) {
    /* Tablet Portrait & Foldable adjustments */
}

/* ─── Expanded (840px – 1199.98px) ───────────────────────────────────────────── */
@media (min-width: 840px) and (max-width: 1199.98px) {
    /* Fixed Counter POS (10"-12") */
}

/* ─── Large (1200px – 1599.98px) ─────────────────────────────────────────────── */
@media (min-width: 1200px) and (max-width: 1599.98px) {
    /* 15.6" POS Station & Large Screens */
}

/* ─── Extra-Large (>= 1600px) ────────────────────────────────────────────────── */
@media (min-width: 1600px) {
    /* Ultrawide monitors & 4K Digital Menu Boards */
}
```

---

## 3. 🏛️ Canonical M3 Pane Patterns (Strict CSS Grid)

Per Socratic architectural standards, **Flexbox is strictly prohibited**. All multi-pane layouts must use CSS Grid.

### Pattern A: Supporting-Pane (POS Checkout Terminal)
Used in `Retail/POS/Pages/Terminal.razor`:

```css
/* Layout Container */
.pos-layout {
    display: grid;
    height: 100vh;
    overflow: hidden;
    gap: 16px;
    padding: 16px;
    box-sizing: border-box;
}

/* Compact: Single column, cart as bottom sheet or toggled overlay */
@media (max-width: 839.98px) {
    .pos-layout {
        grid-template-columns: 1fr;
        grid-template-rows: auto 1fr auto;
    }
    .pos-cart-pane {
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        max-height: 80vh;
        border-radius: var(--md-sys-shape-expressive-sheet);
        transform: translateY(calc(100% - 64px)); /* Peeking header */
        transition: transform var(--md-sys-motion-duration-medium3) var(--md-sys-motion-easing-spring);
    }
    .pos-cart-pane.expanded {
        transform: translateY(0);
    }
}

/* Expanded & Large: Permanent 2-Pane Split */
@media (min-width: 840px) {
    .pos-layout {
        grid-template-columns: minmax(0, 1fr) clamp(340px, 28vw, 440px);
        grid-template-rows: 1fr;
    }
}
```

### Pattern B: List-Detail Layout (Orders, Inventory, Admin)
Used in `Retail/Orders` and `Portal`:

```css
.list-detail-layout {
    display: grid;
    height: 100%;
    gap: 20px;
}

/* Compact (< 600px): Showing either list or detail, not both */
@media (max-width: 599.98px) {
    .list-detail-layout {
        grid-template-columns: 1fr;
    }
    .list-detail-layout.show-detail .list-pane {
        display: none;
    }
    .list-detail-layout:not(.show-detail) .detail-pane {
        display: none;
    }
}

/* Medium & Above (>= 600px): Dual Pane Split */
@media (min-width: 600px) {
    .list-detail-layout {
        grid-template-columns: clamp(280px, 35%, 400px) minmax(0, 1fr);
    }
}
```

### Pattern C: Kiosk Feed & Matrix Layout (Portrait 1080x1920)
Used in `Retail/Kiosk/Pages/Kiosk.razor`:

```css
.kiosk-scaffold {
    display: grid;
    grid-template-rows: auto 1fr auto;
    height: 100vh;
    max-height: 100vh;
    padding: 24px;
    gap: 20px;
    box-sizing: border-box;
}

/* Responsive grid of cards that never overflows horizontally */
.kiosk-product-matrix {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(min(100%, 260px), 1fr));
    gap: 20px;
    overflow-y: auto;
    align-content: start;
}
```

---

## 4. ♿ Kiosk ADA / Section 508 Vertical Reachability Standard

Self-service kiosks with portrait screens (e.g. 21.5"–32" at 1080x1920) present accessibility challenges:
- **Maximum Reach Height**: Under ADA standards, interactive controls must not exceed **48 inches (122 cm)** from the floor for wheelchair access.
- **Minimum Reach Height**: Controls must not be lower than **15 inches (38 cm)** from the floor.

### Architectural Rules for Kiosk Screens:
1. **Interactive Hot Zone in Lower 60%**:
   - The primary checkout CTA ("Pay", "Proceed to Payment", "Call Attendant") MUST be pinned to the lower dock / bottom sheet, NEVER placed at the top of a 1080x1920 viewport.
2. **Top Surface Reserved for Non-Interactive Brand/Status**:
   - The top 20% of portrait kiosks is strictly for brand logo, current step indicator, and passive banners.
3. **Accessibility Mode (ADA Reach Button)**:
   - Provide an optional accessibility trigger in the lower corner that lowers all interactive elements into the bottom 40% of the screen.

```css
/* Kiosk Accessible Bottom Action Bar */
.kiosk-action-dock {
    position: sticky;
    bottom: 0;
    display: grid;
    grid-template-columns: 1fr auto;
    align-items: center;
    gap: 16px;
    padding: 16px 24px;
    background: var(--md-sys-color-surface-container-high);
    border-radius: var(--md-sys-shape-expressive-dock);
    box-shadow: var(--md-sys-elevation-expressive-3);
    backdrop-filter: var(--md-sys-glass-blur);
    -webkit-backdrop-filter: var(--md-sys-glass-blur);
    z-index: 100;
}
```

---

## 5. 🔤 Fluid Typography & Clamping

Never use rigid pixel font-sizes that cause line-wrapping blowout on Compact screens or look microscopic on 4K Large screens. Use CSS `clamp()`:

```css
:root {
    /* Fluid Headline */
    --m3-type-fluid-headline: clamp(1.5rem, 1.2rem + 1.2vw, 2.25rem);
    
    /* Fluid Title */
    --m3-type-fluid-title: clamp(1.1rem, 0.95rem + 0.6vw, 1.5rem);
    
    /* Fluid Body */
    --m3-type-fluid-body: clamp(0.875rem, 0.825rem + 0.25vw, 1.05rem);
    
    /* Fluid Card Minimum Width */
    --m3-card-min-width: clamp(200px, 25vw, 300px);
}
```

---

## 6. 🚫 Forbidden Anti-Patterns (Violations)

1. **Fixed Pixel Widths**: `width: 1200px;` or `min-width: 900px;` (causes horizontal scrollbar on mobile/tablets).
   - ✅ *Fix*: Use `max-width: 100%;` or `width: min(100%, 1200px);`.
2. **Flexbox Usage**: `display: flex;` or `flex-direction: row;`.
   - ✅ *Fix*: Use `display: grid; grid-auto-flow: column;` or `grid-template-columns: ...`.
3. **Non-Standard Breakpoints**: `@media (max-width: 768px)` or `@media (max-width: 992px)`.
   - ✅ *Fix*: Use M3 standard breakpoints (`599.98px`, `839.98px`, `1199.98px`, `1599.98px`).
4. **Hardcoded Heights without Overflow Handling**: `height: 800px;` without `overflow: auto;`.
   - ✅ *Fix*: `min-height: 0; overflow-y: auto;`.
