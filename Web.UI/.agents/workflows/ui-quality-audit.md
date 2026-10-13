---
description: Step-by-step workflow for auditing and fixing rendered UI defects, mobile overflows, theme contrast, and touch targets across Socratic pages and Blazor components.
---

# Workflow: Socratic Rendered UI Quality & Visual Audit

Follow this step-by-step workflow when diagnosing visual defects, verifying responsive layouts, fixing mobile overflows, or validating theme consistency across Socratic pages and components.

---

## 1. Machine Perception & Agent Analytics Pre-flight (CLI)

Before launching browser verification, run the full analytics and preview generation cycle:

```bash
npm --prefix Platform/Shared/DesignSystem/Assets run generate:analytics
```

This single command:
1. Renders Headless Blazor pages into `Platform/Shared/DesignSystem/Assets/wwwroot/preview/*.html`.
2. Generates sensory geometry blueprints (`.blueprint`).
3. Computes typed class constant statistics, adoption rate, and detects raw string class violations in `Platform/Shared/DesignSystem/Assets/wwwroot/analytics/usage/class-constants.json`.

**Gate**:
- In `.razor` and `.cs` files, ensure all CSS classes are referenced through typed `@Classes.*` constants from `Classes.g.cs` (`Assets.Constants.Classes`).
- Run the static token auditor:
```powershell
pwsh -File .agents/skills/ui-quality/scripts/Audit-BlazorUiQuality.ps1
```
- Instantly identifies hardcoded HEX/RGB colors, `::deep` CSS usages, and potential small touch targets (<44px).
- Resolve identified token issues to ensure full Material 3 compliance.

---

## 2. Setup & Target Selection

1. Ensure the Socratic Web host is running (`UI_BASE_URL`, default `https://localhost:6443` or `http://localhost:6080`).
2. Identify target route (e.g. `/`, `/products`, `/booking`, `/checkout`) and target component selector (e.g. `[data-blazor-component=Avatar]`, `[data-testid=product-card]`).

---

## 3. Deterministic Responsive Inspection

Run `inspect_responsive` across all standard viewports:
- Mobile: `320x844`, `360x800`, `390x844`, `430x932`
- Tablet: `768x1024`
- Desktop: `1440x900`

```json
{
  "path": "/products"
}
```
**Gate**: If any viewport returns `horizontalOverflow: true`, stop and address the overflow before subjective visual inspection.

---

## 4. Structural Collision & Overlap Check

Run `find_overlaps` on the target viewport (default mobile `390x844`):
```json
{
  "path": "/products",
  "viewport": { "width": 390, "height": 844 }
}
```
**Gate**: Check if any interactive controls or text blocks overlap. Review stacking context (`position`, `z-index`).

---

## 5. Deep Component Inspection (Geometry + Tokens + Source)

Call `inspect_component` with the component name and target viewport:
```json
{
  "path": "/products",
  "component": "ProductCard",
  "viewport": { "width": 390, "height": 844 },
  "theme": "light"
}
```
Review the resulting report:
- `touch-target-size`: Any button/action smaller than 48x48px (or 44x44px).
- `hardcoded-color-style`: Any inline styles using `#hex` or `rgb(...)` instead of `var(--md-sys-color-*)`.
- `layout-overflow`: Right or left overflow pixels.
- `sourceContext`: Related `.razor` and `.razor.css` files.

---

## 6. Dual-Theme & State Comparison

Run `compare_states` to ensure no visual breakage or contrast loss when switching themes:
```json
{
  "path": "/products",
  "selector": "[data-blazor-component=ProductCard]",
  "viewport": { "width": 390, "height": 844 },
  "stateA": "populated",
  "themeA": "light",
  "stateB": "populated",
  "themeB": "dark"
}
```
Verify that text remains readable against dark surface tokens (`--md-sys-color-surface-container`, `--md-sys-color-on-surface`).

---

## 7. Accessibility Audit (axe-core WCAG 2.1 AA)

Run `analyze_accessibility`:
```json
{
  "path": "/products",
  "selector": "[data-blazor-component=ProductCard]",
  "tags": ["wcag2a", "wcag2aa", "wcag21aa"]
}
```
**Gate**: Zero critical or serious accessibility violations.

---

## 8. Apply Minimal Architectural Fix & Re-Verify

1. Locate the root cause in the appropriate `.razor` or `.razor.css` file in `Platform/Shared` or feature slices.
2. Replace raw CSS class strings with typed `@Classes.*` constants from `Classes.g.cs`.
3. Replace hardcoded dimensions/colors with responsive CSS Grid (strictly NO Flexbox) and Material 3 CSS tokens.
4. Re-run `npm --prefix Platform/Shared/DesignSystem/Assets run generate:analytics` to regenerate `preview/*.html`, blueprints, and update `class-constants.json` statistics.
5. Re-run steps 3–7 to verify that:
   - Original defect is resolved;
   - No regression was introduced in other viewports or themes;
   - Constant adoption rate did not regress.

