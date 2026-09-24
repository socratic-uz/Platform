---
name: socratic-touch-ergonomics
description: Ergonomics, touch target standards, and spatial UX rules for POS, self-service kiosks, mobile registers, and touchscreen devices. USE FOR: touch target sizing (>=48px POS, >=56px Kiosk, >=64px primary CTA), thumb zone layouts, tactile feedback, ghost-click/double-tap prevention, and high-contrast ambient readability.
---

# Socratic Design Engineering: Touch & POS/Kiosk Ergonomics

Socratic powers mission-critical hardware interfaces: cash register terminals (`Retail/POS`), customer self-ordering kiosks (`Retail/Kiosk`), mobile handheld checkout (`Retail/Checkout`), and table ordering. Touch interfaces require strict ergonomics far beyond standard desktop web apps.

---

## 1. 📏 Touch Target Dimensions

| Surface / Context | Minimum Hitbox | Token / Class | Typical Element |
| :--- | :--- | :--- | :--- |
| **Handheld / Mobile POS** | **48 × 48 px** | `--md-sys-touch-target-min`, `.touch-target-pos` | Modifier chips, list items, icon buttons |
| **Kiosk Self-Service** | **56 × 56 px** | `--md-sys-touch-target-kiosk`, `.touch-target-kiosk` | Category tabs, dish cards, cart quantity + / - |
| **Primary Pay / Checkout** | **64 px height** | `--md-sys-touch-target-action`, `.touch-target-action` | "Оплатить", "Завершить заказ", "Сканировать" |
| **Touch Spacing (Gap)** | **>= 12 px** | `--md-sys-touch-gap` | Space between adjacent interactive buttons |

### Rules:
1. **No Tiny Icon Buttons**: Standalone `<Button>` or `<IconButton>` must have a minimum bounding box of 48×48px. If the visual icon is 24px, the tap padding must expand to 48px.
2. **Hitbox Independence**: Never place two touchable buttons closer than 8px apart (recommended 12px) to prevent mis-taps in fast-paced retail or busy restaurant environments.

---

## 2. 📱 Spatial Ergonomics & Thumb Zones

### POS Register Split:
```
┌───────────────────────────────┬────────────────────────┐
│ Catalog & Fast Grid (2fr)     │ Active Ticket (1fr)    │
│ ┌───────────────────────────┐ │ ┌────────────────────┐ │
│ │ Categories (Chips 44px)   │ │ │ Order Items List   │ │
│ ├───────────────────────────┤ │ │ (Scrollable)       │ │
│ │ Product Cards (Min 140px) │ │ ├────────────────────┤ │
│ │ Min Height 120px          │ │ │ Total & Discounts  │ │
│ │ Large Tap Target          │ │ ├────────────────────┤ │
│ │                           │ │ │ Pay Button (56px)  │ │
│ └───────────────────────────┘ │ └────────────────────┘ │
└───────────────────────────────┴────────────────────────┘
```

- **Bottom Dock Alignment**: The highest-frequency action (Checkout / Pay / Charge) MUST be fixed at the bottom right or bottom center, directly accessible to the dominant hand.
- **Scroll Containment**: Always set `overscroll-behavior: contain` and `-webkit-overflow-scrolling: touch` on catalog grids and order receipts to prevent rubber-band bounce.

---

## 3. ⚡ Haptic Simulation & Accidental Tap Prevention

1. **Tactile Spring Compression**:
   Every touchable item must visibly respond on `:active` with instant `scale(0.96)` feedback within `50ms` so the user knows the input was registered.
2. **Debounced Clicks**:
   Critical actions (Payment execution, submit order) must be disabled or debounced immediately upon the first click to prevent double-charging or duplicate order creation:
   ```razor
   <Button Variant="ButtonVariant.Filled" Disabled="@isProcessing" OnClick="HandlePay">
       @if (isProcessing) { <md-circular-progress indeterminate></md-circular-progress> }
       else { <span>@L["Pay Now"]</span> }
   </Button>
   ```

---

## 4. ☀️ Ambient Lighting & Readability

- POS counters often operate under direct overhead spotlights or outdoor market sunlight.
- Text contrast must meet **WCAG 2.1 AA** (minimum 4.5:1 ratio against surface).
- Prices, totals, and barcode strings must use bold typography (`font-weight: 700` or `800`) and high-contrast color tokens:
  - Text: `var(--md-sys-color-on-surface)`
  - Key highlights: `var(--md-sys-color-primary)`

---

## 5. 🔍 Automated Ergonomics Audit

Run the static touch ergonomics auditor:
```powershell
pwsh -File .agents/skills/socratic-touch-ergonomics/scripts/Audit-TouchErgonomics.ps1
```
