---
name: ui-refactor-loop
description: >-
  Data-driven Material 3 Expressive (M3X) CSS styles refactoring loop for stylesheets and layout geometry, driven by Assets tools (stylelint, syntax validator, preview, blueprints).
---

# Workflow: Material 3 Expressive (M3X) CSS Styles Refactoring Loop

Working dir for all styling commands: `src/Frontend/Platform/Shared/DesignSystem/Assets`.
Generated outputs and blueprints are the source of truth. Do NOT grep or read broad swaths of source code.

> **ÐŸÑ€Ð¸Ð¼ÐµÑ‡Ð°Ð½Ð¸Ðµ**: Ð”Ð»Ñ Ñ€ÐµÑ„Ð°ÐºÑ‚Ð¾Ñ€Ð¸Ð½Ð³Ð° Razor-Ñ€Ð°Ð·Ð¼ÐµÑ‚ÐºÐ¸ ÑˆÐ°Ð±Ð»Ð¾Ð½Ð¾Ð² (`*.razor`), ÐºÐ¾Ð½ÑÑ‚Ð°Ð½Ñ‚ Ð¸ code-behind (`*.razor.cs`) Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·ÑƒÐ¹Ñ‚Ðµ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ñ‹Ð¹ Ð²Ð¾Ñ€ÐºÑ„Ð»Ð¾Ñƒ [skills/ux-refactor-loop/SKILL.md](../ux-refactor-loop/SKILL.md).

---

## 0. Refresh (once per iteration, ~2s)

```powershell
npm run refresh                      # codegen (Classes/Icons/Assets/Loc .g.cs) â†’ usage + preview + blueprints
npm run refresh -- --static-only     # fast lint-only pass, blueprints are a placeholder tree
```

`refresh` = `generate:codegen` then `generate:analytics` (codegen first, because `generate-usage` reads `Classes.g.cs`).
Blueprints print a warning when running in static/fallback mode.

---

## 1. Pick target from style analytics, not intuition

Read only these style and geometry reports:
- `wwwroot/analytics/manifest.json` â†’ pages with layout/style violations (`totalViolations > 0`, worst first).
- `wwwroot/analytics/blueprints/<page>.{desktop,tablet,mobile}.blueprint` â†’ geometry tree of the one chosen page (checking for `SiblingCollision`, `BoundaryOverflow`, Bare `1fr`, Flexbox).
- `wwwroot/analytics/usage/class-constants.json` â†’ dead CSS classes (`count: 0`) and unmapped styling classes.

Priority: layout geometry violations (`BoundaryOverflow`, `SiblingCollision`) > bare `1fr` in grid > flexbox usage > hardcoded colors/hex > dead CSS selectors.
Skip pages already marked done in the Ledger below.

---

## 2. Refactor Styles (CSS Architecture & M3X)

1. **Ð¦ÐµÐ½Ñ‚Ñ€Ð°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð¸ Ð´ÐµÑ‚ÐµÑ€Ð¼Ð¸Ð½Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ñ‹Ð¹ ÐºÐ°ÑÐºÐ°Ð´ (@layer & :where(...))**:
   - Ð’ÑÑ ÑÑ‚Ð¸Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ ÑÑ‚Ñ€Ð¾Ð³Ð¾ Ð² `Assets/wwwroot/css/` (ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð° Cascade Layers: `tokens/`, `foundations/`, `layouts/`, `pages/`, `widgets/`, `components/`).
   - Ð¡Ñ‚Ð¸Ð»Ð¸ ÐºÐ¾Ð¼Ð¿Ð¾Ð½ÐµÐ½Ñ‚Ð¾Ð² Ð¸Ð½ÐºÐ°Ð¿ÑÑƒÐ»Ð¸Ñ€ÑƒÑŽÑ‚ÑÑ Ð² Ñ†ÐµÐ»ÐµÐ²Ñ‹Ñ… Ð¼Ð¾Ð´ÑƒÐ»ÑÑ… (`buttons.css`, `cards.css`, `dialogs.css`), ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÑ ÑÐ»Ð°Ð±ÑƒÑŽ ÑÐ²ÑÐ·Ð°Ð½Ð½Ð¾ÑÑ‚ÑŒ.
   - Ð‘Ð°Ð·Ð¾Ð²Ñ‹Ðµ Ð¿Ñ€Ð¸Ð¼Ð¸Ñ‚Ð¸Ð²Ñ‹ Ð¾Ð±Ð½ÑƒÐ»ÑÑŽÑ‚ ÑÐ¿ÐµÑ†Ð¸Ñ„Ð¸Ñ‡Ð½Ð¾ÑÑ‚ÑŒ Ñ‡ÐµÑ€ÐµÐ· :where(...) (0, 0, 0), ÑƒÑÑ‚Ñ€Ð°Ð½ÑÑ ÐºÐ¾Ð½Ñ„Ð»Ð¸ÐºÑ‚Ñ‹ ÑÐµÐ»ÐµÐºÑ‚Ð¾Ñ€Ð¾Ð² Ð±ÐµÐ· Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ð½Ð¸Ñ !important.
   - ÐŸÐ¾Ð»Ð½Ñ‹Ð¹ Ð·Ð°Ð¿Ñ€ÐµÑ‚ Ð½Ð° `.razor.css` (Ð·Ð° Ð¸ÑÐºÐ»ÑŽÑ‡ÐµÐ½Ð¸ÐµÐ¼ QuickGrid Ð¸ Markdown), Ð¸Ð½Ð»Ð°Ð¹Ð½ `style="..."`, Ð±Ð»Ð¾ÐºÐ¸ `<style>` Ð¸ TailwindCSS.
   - ÐŸÐ¾Ð»Ð½Ñ‹Ð¹ Ð·Ð°Ð¿Ñ€ÐµÑ‚ `!important` (Ð·Ð° Ð¸ÑÐºÐ»ÑŽÑ‡ÐµÐ½Ð¸ÐµÐ¼ 3 Ð»ÐµÐ³Ð¸Ñ‚Ð¸Ð¼Ð½Ñ‹Ñ… ÑÐ»ÑƒÑ‡Ð°ÐµÐ²: `prefers-reduced-motion`, `[hidden]`, Ð¸Ð·Ð¾Ð»ÑÑ†Ð¸Ñ Ð¸ÐºÐ¾Ð½Ð¾Ðº).
2. **Pure CSS Grid (Ð¡Ñ‚Ñ€Ð¾Ð³Ð¸Ð¹ Ð·Ð°Ð¿Ñ€ÐµÑ‚ Flexbox)**:
   - Ð’ÑÐµ Ð»ÐµÐ¹Ð°ÑƒÑ‚Ñ‹, ÐºÐ°Ñ€Ñ‚Ð¾Ñ‡ÐºÐ¸, Ð¿Ð°Ð½ÐµÐ»Ð¸ Ð¸ ÑÑ‚ÐµÐºÐ¸ ÑÑ‚Ñ€Ð¾ÑÑ‚ÑÑ Ð¸ÑÐºÐ»ÑŽÑ‡Ð¸Ñ‚ÐµÐ»ÑŒÐ½Ð¾ Ð½Ð° `display: grid`.
   - ÐšÐ¾Ð½Ñ‚Ñ€Ð°ÐºÑ‚ Ð³ÐµÐ¾Ð¼ÐµÑ‚Ñ€Ð¸Ð¸ Ð²ÐµÑ€ÑÑ‚ÐºÐ¸: ÑÑ‚Ñ€Ð¾Ð³Ð¾ `minmax(0, 1fr)` Ð´Ð»Ñ Ð³Ð¸Ð±ÐºÐ¸Ñ… ÐºÐ¾Ð»Ð¾Ð½Ð¾Ðº (Ð·Ð°Ð¿Ñ€ÐµÑ‚ Ð³Ð¾Ð»Ð¾Ð³Ð¾ `1fr`).
   - ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ð¹ `min-width: 0;` Ð½Ð° Ð²ÑÐµÑ… ÑÑ‡ÐµÐ¹ÐºÐ°Ñ… ÑÐµÑ‚ÐºÐ¸ Ñ Ñ‚ÐµÐºÑÑ‚Ð¾Ð²Ñ‹Ð¼ Ð¿ÐµÑ€ÐµÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸ÐµÐ¼ (`text-overflow: ellipsis`).
3. **M3 Expressive (M3X) Ð²Ð¸Ð·ÑƒÐ°Ð»ÑŒÐ½Ñ‹Ð¹ ÑÑ‚Ð¸Ð»ÑŒ Ð¸ Ñ‚Ð¾ÐºÐµÐ½Ð¸Ð·Ð°Ñ†Ð¸Ñ**:
   - Ð’Ñ‹Ñ€Ð°Ð·Ð¸Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ñ€Ð°Ð´Ð¸ÑƒÑÑ‹: ÐºÐ°Ñ€Ñ‚Ð¾Ñ‡ÐºÐ¸ `24px` (`--md-sys-shape-expressive-card`), ÐºÐ½Ð¾Ð¿ÐºÐ¸ `20px`, Ñ‡Ð¸Ð¿Ñ‹/pills `9999px`, Ð¼Ð¾Ð´Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¾ÐºÐ½Ð° `32px`.
   - ÐŸÑ€ÑƒÐ¶Ð¸Ð½Ð½Ð°Ñ Ñ„Ð¸Ð·Ð¸ÐºÐ° Ð°Ð½Ð¸Ð¼Ð°Ñ†Ð¸Ð¹: `cubic-bezier(0.175, 0.885, 0.32, 1.275)`, Ñ‚Ð°ÐºÑ‚Ð¸Ð»ÑŒÐ½Ñ‹Ð¹ Ð¾Ñ‚ÐºÐ»Ð¸Ðº `:active scale(0.96)`.
   - Ð¦Ð²ÐµÑ‚Ð° Ð¸ Ñ‚ÐµÐ¼Ñ‹: Ñ‚Ð¾Ð»ÑŒÐºÐ¾ M3-Ñ‚Ð¾ÐºÐµÐ½Ñ‹ `var(--md-sys-color-*)`, WCAG 2.1 AA Ð² Light Ð¸ Dark Ñ‚ÐµÐ¼Ð°Ñ… (`:root[data-theme="dark"]`, `:root[data-theme="light"]`).
4. **CSS Container Queries (`@container`)**:
   - ÐœÐ¸ÐºÑ€Ð¾-Ð°Ð´Ð°Ð¿Ñ‚Ð¸Ð²Ð½Ð¾ÑÑ‚ÑŒ Ð²Ð¸Ð´Ð¶ÐµÑ‚Ð¾Ð², Ð¼Ð¾Ð´Ð°Ð»Ð¾Ðº Ð¸ ÐºÐ°Ñ€Ñ‚Ð¾Ñ‡ÐµÐº Ñ‡ÐµÑ€ÐµÐ· `@container` Ð²Ð¼ÐµÑÑ‚Ð¾ Ð³Ð»Ð¾Ð±Ð°Ð»ÑŒÐ½Ñ‹Ñ… Ð¼ÐµÐ´Ð¸Ð°-Ð·Ð°Ð¿Ñ€Ð¾ÑÐ¾Ð² Ð¾ÐºÐ½Ð° Ð±Ñ€Ð°ÑƒÐ·ÐµÑ€Ð°.
5. **Ð­Ñ€Ð³Ð¾Ð½Ð¾Ð¼Ð¸ÐºÐ° Ñ‚Ð°Ñ‡-Ð¸Ð½Ñ‚ÐµÑ€Ñ„ÐµÐ¹ÑÐ¾Ð²**:
   - POS: `>= 48x48px`, Kiosk: `>= 56x56px`, Checkout: `>= 64px`, Ð¾Ñ‚ÑÑ‚ÑƒÐ¿Ñ‹ `>= 12px`.
   - ÐžÑ‚ÑÑƒÑ‚ÑÑ‚Ð²Ð¸Ðµ Ð³Ð¾Ñ€Ð¸Ð·Ð¾Ð½Ñ‚Ð°Ð»ÑŒÐ½Ð¾Ð³Ð¾ ÑÐºÑ€Ð¾Ð»Ð»Ð° (`scrollWidth <= clientWidth`) Ð²Ð¾ Ð²ÑÐµÑ… Ñ€Ð°Ð·Ñ€ÐµÑˆÐµÐ½Ð¸ÑÑ… (`320px` - `1920px`).

---

## 3. Verify (machine gates, all must pass)

```powershell
npm run validate:syntax
npm run lint:css
npm run build                        # Ð²Ð°Ð»Ð¸Ð´Ð°Ñ†Ð¸Ñ ÑÐ¸Ð½Ñ‚Ð°ÐºÑÐ¸ÑÐ° CSS, ÑÐ±Ð¾Ñ€ÐºÐ° Web Components JS Ð¸ Ð³ÐµÐ½ÐµÑ€Ð°Ñ†Ð¸Ñ C#-ÐºÐ¾Ð½ÑÑ‚Ð°Ð½Ñ‚
npm run generate:codegen ; npm run generate:analytics
dotnet build src/Frontend/Platform/Web.UI/Web.UI/Web.UI.csproj -c Debug
```

Gates:
- build: 0 errors, 0 new warnings;
- `manifest.json` â†’ target page `totalViolations == 0`;
- `lint:css` â†’ 0 stylelint errors;
- `class-constants.json` â†’ no newly dead constants, no new orphaned classes;
- blueprints â†’ 0 `BoundaryOverflow` Ð¸ 0 `SiblingCollision`.

If preview HTML is missing for the page, add it to `Tools/PreviewGenerator`, not hand-written HTML.

---

## 4. Record and continue


Append one line to the Ledger. Do not commit/push without a direct user request.
Then go to step 1 automatically for the next page; stop only on a failed gate you cannot fix, or when the manifest has no page with violations and no raw-md candidates.

## Known limits of the tooling (treat as caveats)

- `generate-blueprints.mjs` with `--static-only` (or no Playwright) emits a FAKE hardcoded layout tree and only line-based regex rules (flex, hex, bare `1fr`). Use it only as a lint; for geometry, run live mode.
- Live mode uses Playwright installed in `Assets/node_modules/playwright`.
- Blueprint violations only flag `SiblingCollision` / `BoundaryOverflow` as warnings; the manifest counts all types.

## Ledger

- SeatDesigner (Toolbar, Canvas, Page)
- QrDesigner (Page, QrCode)
- Kiosk (Page, CategoryChipSet, ProductCard, Dialogs)
- Payment (Page, PlanCards, TrustFooter, ResponsiveHeader)
- OrderItems (Page, ItemCards, SummaryAside, ResponsiveHeader)
- Help (Page, Hero, CategoryChips, FaqAccordion, SupportCards)
- Analytics (Page, Toolbar, KpiCards, PaymentBreakdown, ExplorerGrid)
- Landing (Page, Hero, OrgCards, CtaButtons, SpatialSection)
- Map (Page, ControlsPanel, SocraticMap, FloorSwitcher, SearchBar)
- About (Page, Hero, VideoShowcase, PillarsSection, ValuesGrid, ActionsRow)
- QR (Page, Sidebar, QrStylingCustomizer, ProductLists)
- Terminal (Page, HeaderActions, CatalogSearch, InvoiceSidebar)
- Detector (Page, SidebarTabs, ConfigForm, CameraControls)
- Home (Page, HeaderSearch, OrgCards, SectionContent)
- Kiosk (HeaderSearch, Complete 0-violation baseline)

