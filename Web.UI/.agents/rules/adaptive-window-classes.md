# Socratic Engineering Rule: Material Design 3 Adaptive Window Classes

## 1. 🎯 Mandatory Breakpoint Standards

All Socratic frontend components (`.razor`, `.razor.css`, `.css`) must strictly adhere to the **Google Material Design 3 Window Size Classes**.

### Canonical Breakpoint Values:
- **Compact**: `< 600px` (Phone, Mobile POS) -> `@media (max-width: 599.98px)`
- **Medium**: `600px – 839.98px` (Tablet Portrait, Foldable) -> `@media (min-width: 600px) and (max-width: 839.98px)`
- **Expanded**: `840px – 1199.98px` (Small POS Countertop 10"-12", Landscape Tablet) -> `@media (min-width: 840px) and (max-width: 1199.98px)`
- **Large**: `1200px – 1599.98px` (Standard 15.6" POS, Kiosk 1080p) -> `@media (min-width: 1200px) and (max-width: 1599.98px)`
- **Extra-Large**: `$\ge 1600px$` (Back-office Desktop, Ultrawide, 4K) -> `@media (min-width: 1600px)`

> [!CAUTION]
> **Strict Prohibition**: Ad-hoc breakpoints like `768px`, `992px`, `1024px`, or `1366px` are strictly forbidden. Use M3 standard breakpoint queries.

---

## 2. 🏛️ Pure CSS Grid Architecture (Strictly NO Flexbox)

1. All multi-pane, card grid, and action layouts MUST use CSS Grid (`display: grid`).
2. Flexbox (`display: flex`) is prohibited across all Socratic UI modules.
3. Card grids must use `grid-template-columns: repeat(auto-fill, minmax(min(100%, <size>), 1fr));` to prevent horizontal clipping.

---

## 3. 🛡️ Horizontal Overflow Prevention

1. **No Fixed Pixel Widths**: Never declare fixed `width: 800px;` or `min-width: 600px;` on root containers.
2. **Fluid Clamping**: Always wrap sizing in `min(100%, ...)` or `clamp(<min>, <val>, <max>)`.
3. **Viewport Safety**: Ensure all views render cleanly without horizontal scrollbars at 360px width.

---

## 4. ♿ Self-Service Kiosk Vertical Accessibility (ADA / Section 508)

For portrait kiosks (1080x1920 or higher):
1. **Interactive Hot Zone**: All primary call-to-action buttons ("Pay", "Checkout", "Help") MUST reside in the bottom 60% of the screen.
2. **Top Surface**: The top 20% is strictly reserved for non-interactive branding, step indicators, and banners.
3. **Touch Targets**: Minimum 56px for secondary targets, 64px for primary checkout actions.

---

## 5. 🔍 Automated Static Audit

Run `Audit-AdaptiveLayout.ps1` before committing any UI code:
```powershell
pwsh -File .agents/skills/socratic-adaptive-layout/scripts/Audit-AdaptiveLayout.ps1 -Path src/Frontend
```
Violations will fail CI/CD quality gates.
