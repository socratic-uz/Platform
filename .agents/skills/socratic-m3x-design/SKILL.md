---
name: socratic-m3x-design
description: Authoritative guide for Material Design 3 Expressive (M3X) engineering in Socratic Blazor applications. USE FOR: expressive shape scales (24px cards, 32px docks/dialogs, pill 9999px, asymmetric shapes), spring physics motion curves, colored ambient elevations, liquid frosted glass, top surface sheen, and M3X component authoring (Card, Button, Chip). DO NOT USE FOR: standard flat Material 2 or TailwindCSS.
---

# Socratic Design Engineering: Material Design 3 Expressive (M3X)

Material Design 3 Expressive (M3X) is Socratic's foundational design language. It evolves standard Material Design into a tactile, vivid, and deeply responsive visual system tailored for modern web apps, high-throughput retail terminals (POS), self-service kiosks, and computer vision surfaces.

---

## 1. 📐 Expressive Shape Scale

M3X introduces expressive shapes that reflect brand identity, improve tap targets, and visually distinguish functional surfaces.

### Core Shape Tokens:
```css
/* Socratic Platform expressive.css */
--md-sys-shape-expressive-card: 24px;               /* Standard elevated/filled cards */
--md-sys-shape-expressive-button: 20px;             /* Rounded interactive buttons */
--md-sys-shape-expressive-pill: 9999px;             /* Pill buttons, search bars, chips */
--md-sys-shape-expressive-dock: 32px;               /* Floating bottom/side toolbars */
--md-sys-shape-expressive-dialog: 32px;             /* Expressive modal dialogues */
--md-sys-shape-expressive-sheet: 32px 32px 0 0;     /* Bottom navigation sheets */
--md-sys-shape-expressive-asymmetric-start: 28px 10px 28px 10px; /* Highlighting badges/cards */
--md-sys-shape-expressive-asymmetric-end: 10px 28px 10px 28px;
--md-sys-shape-expressive-bubble: 24px 24px 6px 24px;            /* Chat bubbles & tooltips */
```

### Component Usage:
```razor
<!-- Standard Expressive Card in Material.Web -->
<Card Expressive="true" Variant="CardVariant.Elevated">
    <h3>@Product.Name</h3>
</Card>

<!-- Pill Button -->
<Button Variant="ButtonVariant.Filled" Pill="true" Expressive="true">
    <Icon Name="shopping_cart" Slot="icon" />
    @L["Pay Now"]
</Button>
```

---

## 2. 🌊 Spring Physics & Choreographed Motion

M3X completely replaces artificial linear and standard ease transitions with **natural spring physics**:

```css
/* Spring Curves */
--md-sys-motion-easing-spring: cubic-bezier(0.175, 0.885, 0.32, 1.275);
--md-sys-motion-easing-spring-snappy: cubic-bezier(0.2, 1.3, 0.4, 1.0);
--md-sys-motion-easing-spring-soft: cubic-bezier(0.3, 1.15, 0.5, 1.0);
```

### Micro-Interaction Rules:
1. **Hover Lift**: On interactive cards and buttons, apply subtle lift with spring expansion:
   ```css
   .m3-expressive-card:hover {
       transform: translateY(-3px) scale(1.004);
       box-shadow: var(--md-sys-elevation-expressive-2), var(--md-sys-glow-primary-subtle);
   }
   ```
2. **Tactile Spring Press**: On active press, compress slightly to simulate physical resistance:
   ```css
   .m3-expressive-card:active {
       transform: translateY(-1px) scale(0.97);
       transition-duration: var(--md-sys-motion-duration-short1);
   }
   ```
3. **Reduced Motion Accessibility**: Always wrap motion in `@media (prefers-reduced-motion: reduce)`:
   ```css
   @media (prefers-reduced-motion: reduce) {
       *, *::before, *::after {
           animation-duration: 0.01ms !important;
           transition-duration: 0.01ms !important;
           transform: none !important;
       }
   }
   ```

---

## 3. 💡 Colored Ambient Elevation & Surface Sheen

In M3X, shadows are **not dull monochrome gray**. They blend with ambient primary brand glow and feature top-sheen lighting:

### Ambient Elevation Tokens:
```css
--md-sys-elevation-expressive-1: 0 2px 10px -2px rgba(0, 0, 0, 0.15), 0 1px 3px 0 rgba(0, 0, 0, 0.12);
--md-sys-elevation-expressive-2: 0 6px 20px -4px rgba(0, 0, 0, 0.22), 0 2px 8px 0 rgba(0, 0, 0, 0.16);
--md-sys-elevation-expressive-3: 0 12px 32px -6px rgba(0, 0, 0, 0.28), 0 4px 14px 0 rgba(0, 0, 0, 0.20);
--md-sys-elevation-expressive-4: 0 18px 44px -8px rgba(0, 0, 0, 0.35), 0 6px 20px 0 rgba(0, 0, 0, 0.24);

/* Brand Glow */
--md-sys-glow-primary-subtle: 0 0 16px -2px var(--md-sys-color-primary);
--md-sys-glow-primary-bold: 0 0 28px 2px var(--md-sys-color-primary);
```

### Top Surface Sheen & Frosted Glass:
```css
/* Frosted glass backdrop */
backdrop-filter: var(--md-sys-glass-blur);
-webkit-backdrop-filter: var(--md-sys-glass-blur);

/* Top sheen 1px highlight */
box-shadow: inset 0 1px 1px 0 rgba(255, 255, 255, 0.15);
```

---

## 4. 🎯 Token Purity Standard

- **Zero Hardcoded Colors**: Never use `#hex` or `rgba(r, g, b, a)` directly in component CSS.
- **Pure CSS Variables**:
  - `var(--md-sys-color-surface)`
  - `var(--md-sys-color-surface-container)`
  - `var(--md-sys-color-primary)`
  - `var(--md-sys-color-on-primary)`
  - `var(--md-sys-color-outline-variant)`
- Run `Audit-ThemeColors.ps1` and `Audit-BlazorUiQuality.ps1` to ensure compliance.
