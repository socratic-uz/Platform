---
description: Data-driven M3 Expressive refactoring loop for Razor/CSS pages, driven by Assets generators (codegen, usage, preview, blueprints).
---

# Workflow: M3 Expressive Refactoring Loop

Working dir for all commands: `src/Frontend/Platform/Shared/DesignSystem/Assets`.
Generated outputs are the source of truth. Do NOT grep or read broad swaths of .razor/.css to "find" things.

## 0. Refresh (once per iteration, ~2s)

```powershell
npm run refresh                      # codegen (Classes/Icons/Assets/Loc .g.cs) → usage + preview + blueprints
npm run refresh -- --static-only     # fast lint-only pass, blueprints are a placeholder tree
```

`refresh` = `generate:codegen` then `generate:analytics` (codegen first, because `generate-usage` reads `Classes.g.cs`).
Blueprints now print a warning when running in static/fallback mode.

## 1. Pick target from data, not intuition

Read only these (small JSON, not source):
- `wwwroot/analytics/usage/class-constants.json` → constants with `count: 0` are dead; raw string classes with no constant are candidates to migrate.
- `wwwroot/analytics/usage/components.json` → raw `md-*` web components used where a Razor wrapper in `Material.Web/Components` exists.
- `wwwroot/analytics/manifest.json` → pages with `totalViolations > 0` (worst first).
- `wwwroot/analytics/blueprints/<page>.{desktop,tablet,mobile}.blueprint` → geometry tree of the one chosen page.

Priority: violations > raw md-* with a wrapper > magic-string classes > dead constants > cosmetic nesting.
Skip pages already marked done in the Ledger below.

## 2. Refactor (one page per iteration)

1. **Базовые компоненты**: строго использовать Razor-обертки из `Material.Web` (`<Button>`, `<TextField>`, `<MaterialDialog>`, `<Chip>`, `<Badge>`, `<Card>`) вместо сырых `@material/web` или самодельных тегов. Нативный `@material/web` допускается только если в `Material.Web` нет подходящей обертки.
2. **Импорт констант (`using static`)**: подключать `@using static` для доменных классов, иконок и локализации, чтобы разметка оставалась компактной:
   - `@using static Assets.Constants.Classes.<Domain>` ➔ `@Container`, `@Toolbar`
   - `@using static Assets.Constants.Icons` ➔ `@Add`, `@Search`
   - `@using static Assets.Constants.Loc.<Domain>` ➔ `@L[NewProduct]`, `@L[Save]`
   - Категорический запрет на длинные громоздкие пути вроде `@Classes.Domain.Subdomain.Class` и сырые строковые литералы.
3. **HTML5 Семантика и Zero-Wrapper**: схлопывать паразитные `<div>`; использовать `<main>`, `<header>`, `<section>`, `<article>`, `<aside>` с прямым управлением сеткой CSS Grid.
4. **CSS & Assets**: стили только в W3C модулях `Assets/css/` (single-declaration, `:where(...)`, zero `!important`, zero inline `style`).
5. **Чистка мертвого кода**: удалять неиспользуемые стили, блоки `<style>` и неиспользуемые `using`.

## 3. Verify (machine gates, all must pass)

```powershell
npm run validate:syntax
npm run lint:css
npm run generate:codegen ; npm run generate:analytics
dotnet build <affected csproj> -c Debug
```

Gates:
- build: 0 errors, 0 new warnings;
- `manifest.json` → target page `totalViolations == 0`;
- `class-constants.json` → no newly dead constants, no new magic strings;
- `components.json` → raw `md-*` count for the page did not grow.

If preview HTML is missing for the page, add it to `Tools/PreviewGenerator`, not hand-written HTML.

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
