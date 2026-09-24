---
name: socratic-ui-quality
description: Comprehensive rendered UI quality, visual regression, responsiveness, and design token inspection engine for Socratic Blazor applications (Web.UI, Platform/Shared, MAUI Blazor Hybrid). USE FOR: verifying actual rendered browser state (DOM, computed styles, geometry, scroll dimensions, bounding rects), responsive matrix checks (320px-1920px), finding layout overflows and overlapping elements, validating Google Material Design 3 tokens (--md-sys-color-*, --md-sys-shape-*), eliminating hardcoded inline HEX/RGB colors, verifying touch target sizes (>=48x48px for POS/kiosk/mobile), running WCAG 2.1 AA accessibility audits via axe-core, and comparing Light/Dark theme states. DO NOT USE FOR: reviewing backend database or business logic.
---

# Socratic UI Quality Engine & Inspection Methodology

This skill defines the authoritative methodology and tooling for diagnosing, measuring, and continuously improving the actual rendered user interface across the **Socratic** ecosystem (**Blazor Web App**, **.NET MAUI Blazor Hybrid**, and the **`Platform/Shared`** component library).

---

## 1. Core Philosophy: Browser as Source of Truth

Always distinguish between:
1. **Source Code**: Razor templates, `.razor.cs` code-behind, `.razor.css` isolated styles, and token definitions.
2. **Actual Rendered UI**: Live DOM, Shadow DOM, computed CSSOM styles, element geometry (`getBoundingClientRect`), viewport boundaries, accessibility tree, and visual appearance.

> **CRITICAL RULE**: Do NOT assume Razor/CSS source represents the final appearance.
> The rendered browser state is the primary source of truth for visual, layout, and responsive defects.

---

## 2. Socratic Design System & Technical Constraints

All UI components and pages in Socratic must comply with these architectural constraints:

1. **Google Material Design 3 Web Components**:
   - Use official `@material/web` elements (`<md-filled-button>`, `<md-outlined-text-field>`, `<md-icon>`, `<md-switch>`, `<md-dialog>`, `<md-chip-set>`).
   - Respect Shadow DOM encapsulation and theme inheritance.
2. **Strict Vanilla CSS & Design Tokens (NO TailwindCSS)**:
   - Tailwind utility classes are strictly forbidden.
   - All colors must use M3 tokens from `colors.css`:
     - Surface: `var(--md-sys-color-surface)`, `var(--md-sys-color-surface-container)`
     - Content/Text: `var(--md-sys-color-on-surface)`, `var(--md-sys-color-on-surface-variant)`
     - Primary: `var(--md-sys-color-primary)`, `var(--md-sys-color-on-primary)`
     - Borders: `var(--md-sys-color-outline)`, `var(--md-sys-color-outline-variant)`
   - No hardcoded HEX (`#1e293b`, `#00ffb3`) or RGB values in inline `style="..."`.
3. **Spatial & Typography Scale**:
   - Spacing scale based on 4px/8px grid: `4px`, `8px`, `12px`, `16px`, `24px`, `32px`, `48px`. Avoid arbitrary magic numbers (`7px`, `13px`, `19px`, `23px`).
4. **Touch Target Size for Mobile & POS/Kiosk**:
   - Minimum interactive touch targets: **48x48px** for POS terminals, kiosks, and mobile screens (minimum 44x44px per WCAG 2.1 AA).
5. **Zero-Flicker (Anti-FOUC) & Blazor Hydration**:
   - Components run under `InteractiveAuto` or `InteractiveServer`. Styles and themes must settle without layout shifts (CLS) or flash of unstyled content during hydration.

---

## 3. Inspection Strategy & State Matrix

Never judge a component or page from a single viewport or screenshot. Always inspect a representative matrix:

### Viewports Matrix
- **Mobile Phones**: `320x844` (critical min-width), `360x800`, `390x844`, `430x932`
- **Tablets & POS Terminals**: `768x1024`, `1024x768`
- **Desktops**: `1440x900`, `1920x1080`

### Theme Matrix
- **Light Theme**
- **Dark Theme** (verify text contrast >= 4.5:1 against surfaces)

### UI State Matrix
- **Loading**: `SkeletonLoader.razor` or progress indicators
- **Empty**: Zero items / empty cart / no results
- **Populated**: Realistic dynamic data
- **Error**: API error messages, invalid validation states
- **Disabled / Interactive**: Focus, hover, active, pressed states

---

## 4. Deterministic Analysis Order

Always analyze problems in this strict sequence:

```
1. STRUCTURAL CORRECTNESS (Overflow, overlap, clipping, CSS Grid layout, zero flexbox)
   ↓
2. ALIGNMENT (Edges, centers, baselines, vertical/horizontal alignment)
   ↓
3. SPACING (4px/8px scale, token consistency, gap, padding, margin)
   ↓
4. TYPOGRAPHY (Font size, line height, weights, truncation, readability)
   ↓
5. RESPONSIVE BEHAVIOR (Stacking, wrapping, collapsing, touch targets)
   ↓
6. VISUAL HIERARCHY (Primary action, headings, alerts, clarity of purpose)
   ↓
7. VISUAL QUALITY & POLISH (Contrast, elevation, rhythm, Dark/Light consistency)
```

> **Screenshots rule**: Do NOT analyze screenshots first. Run deterministic geometric tools (`inspect_responsive`, `find_overlaps`, `inspect_component`) first. Capture screenshots only as evidence for suspicious or broken states.

---

## 5. Available MCP Tools (`socratic-ui-quality`)

Use the `socratic-ui-quality` MCP server tools:

| Tool | Purpose | Key Parameters |
| :--- | :--- | :--- |
| `inspect_responsive` | Deterministic overflow and scroll metrics across all standard viewports (320px–1920px). | `path: "/products"` |
| `inspect_ui` | Deep DOM, geometry, computed styles, scroll dimensions, and runtime errors. | `path: "/"`, `selector: ".cart-summary"` |
| `find_overlaps` | Mathematical bounding box intersection detector for sibling elements. | `path: "/booking"`, `viewport: { width: 390, height: 844 }` |
| `inspect_component` | Unified report: browser geometry + Blazor source context (.razor/.razor.css) + touch-target + token check. | `path: "/products"`, `component: "ProductCard"` |
| `compare_states` | Pixelmatch visual diff + DOM geometry diff between two states/themes. | `stateA: "populated"`, `themeA: "light"`, `stateB: "error"`, `themeB: "dark"` |
| `analyze_accessibility` | Runs axe-core WCAG 2.1 AA audits on the full page or component subtree. | `selector: "[data-blazor-component=Avatar]"`, `tags: ["wcag2aa"]` |
| `run_ui_scenario` | Safe declarative interactions (`click`, `fill`, `press`, `setState`, `setTheme`, `waitFor`). | `actions: [{ type: "click", selector: "button" }]` |
| `screenshot` | Visual PNG capture for human confirmation and evidence. | `viewport: { width: 390, height: 844 }` |

---

## 6. Socratic Local Tooling Architecture

All UI quality tools are fully integrated and self-contained inside the Socratic repository:

1. **.NET 10 Source Analyzer**:
   - Path: [src/Tools/BlazorUiQuality.Analyzer](file:///c:/Users/owner/source/repos/Socratic/src/Tools/BlazorUiQuality.Analyzer)
   - CLI: `dotnet run --project src/Tools/BlazorUiQuality.Analyzer -- --source-root src/Frontend/Platform/Shared [--format human|json]`
   - Analyzers: `@rendermode`, `[Parameter]`, `::deep` CSS, hardcoded HEX/RGB colors, and touch target risks.
   - 100% Native AOT & Trimming compliant (zero IL warnings).

2. **Local Playwright MCP Server**:
   - Path: [tools/socratic-ui-mcp](file:///c:/Users/owner/source/repos/Socratic/tools/socratic-ui-mcp)
   - Built via `npm run build` (`dist/index.js`).
   - Configured in [.agents/mcp_config.json](file:///c:/Users/owner/source/repos/Socratic/.agents/mcp_config.json).
   - Features: Shadow DOM traversal for `@material/web`, automatic touch target validation (<48x48px on mobile/tablet), inline HEX token violation detection.

3. **Fast Standalone CLI Script**:
   - Path: [.agents/skills/socratic-ui-quality/scripts/Audit-BlazorUiQuality.ps1](file:///c:/Users/owner/source/repos/Socratic/.agents/skills/socratic-ui-quality/scripts/Audit-BlazorUiQuality.ps1)
   - Run from terminal:
     ```powershell
     pwsh -File .agents/skills/socratic-ui-quality/scripts/Audit-BlazorUiQuality.ps1
     ```
   - Automatically builds the analyzer if needed, runs against `src/Frontend/Platform/Shared` (or specified target), and outputs a color-coded summary of flagged components.

4. **Authentication on Protected Pages**:
   - **Cookie Injection**: Set `UI_AUTH_COOKIE="AccessToken=<token>"` in [.agents/mcp_config.json](file:///c:/Users/owner/source/repos/Socratic/.agents/mcp_config.json) or environment.
   - **Storage State**: Set `UI_STORAGE_STATE="path/to/storageState.json"` with pre-authenticated cookies/session.
   - **Bearer Token**: Set `UI_AUTH_HEADER="Bearer <token>"`.
   - **Interactive UI Scenario**: Use `run_ui_scenario` to navigate to `/signin`, fill inputs, submit, and proceed to protected pages.

---

## 7. Standard Issue Reporting Format

For every detected problem, report using this structured format:

### Problem
Concise description of the objective visual or layout defect.

### Evidence
Measured values: Viewport (`width x height`), state, theme, element selector, `scrollWidth` vs `clientWidth`, `rightOverflow` in px, or contrast ratio.

### Root Cause
Underlying CSS/Razor architectural cause (e.g. usage of forbidden flexbox instead of CSS grid, fixed width without `minmax()`, hardcoded color `#1e293b` instead of token).

### Fix
Minimal architectural source change preserving design tokens and component reusability.

### Verification
Viewports and states re-tested after applying the fix.

### Regression
Confirmation that no other viewport, theme, or state was negatively affected.

---

## 8. The Quality Loop

Always think and act in this continuous cycle:

```
RENDER → INSPECT → MEASURE → ANALYZE → ROOT CAUSE → FIX SOURCE → RE-RENDER → VERIFY → REGRESSION CHECK
```
