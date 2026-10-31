---
name: ui-refactor-loop
description: >-
  Data-driven Material 3 Expressive (M3X) CSS styles refactoring loop for stylesheets and layout geometry, driven by Assets tools (stylelint, syntax validator, preview, blueprints).
---

# Workflow: Material 3 Expressive (M3X) CSS Styles Refactoring Loop

Working dir for all styling commands: `src/Frontend/Platform/Shared/DesignSystem/Assets`.
Generated outputs and blueprints are the source of truth. Do NOT grep or read broad swaths of source code.

> **Примечание**: Для рефакторинга Razor-разметки шаблонов (`*.razor`), констант и code-behind (`*.razor.cs`) используйте отдельный воркфлоу [skills/ux-refactor-loop/SKILL.md](../ux-refactor-loop/SKILL.md).

---

## 0. Refresh (once per iteration, ~2s)

```powershell
npm run refresh                      # codegen (Classes/Icons/Assets/Loc .g.cs) → usage + preview + blueprints
npm run refresh -- --static-only     # fast lint-only pass, blueprints are a placeholder tree
```

`refresh` = `generate:codegen` then `generate:analytics` (codegen first, because `generate-usage` reads `Classes.g.cs`).
Blueprints print a warning when running in static/fallback mode.

---

## 1. Pick target from style analytics, not intuition

Read only these style and geometry reports:
- `wwwroot/analytics/manifest.json` → pages with layout/style violations (`totalViolations > 0`, worst first).
- `wwwroot/analytics/blueprints/<page>.{desktop,tablet,mobile}.blueprint` → geometry tree of the one chosen page (checking for `SiblingCollision`, `BoundaryOverflow`, Bare `1fr`, Flexbox).
- `wwwroot/analytics/usage/class-constants.json` → dead CSS classes (`count: 0`) and unmapped styling classes.

Priority: layout geometry violations (`BoundaryOverflow`, `SiblingCollision`) > bare `1fr` in grid > flexbox usage > hardcoded colors/hex > dead CSS selectors.
Skip pages already marked done in the Ledger below.

---

## 2. Refactor Styles (CSS Architecture & M3X)

1. **Централизация и детерминированный каскад (@layer & :where(...))**:
   - Вся стилизация строго в `Assets/wwwroot/css/` (7-слойная структура: `foundations/`, `tokens/`, `components/`, `widgets/`, `layouts/`, `pages/`, `features/`).
   - Идентичные CSS-свойства объединяются через мастер-селекторы `:where(...)` с нулевой специфичностью `(0, 0, 0)`.
   - *Selector Append*: при добавлении новых классов селектор дописывается через запятую в существующий блок `:where(...)` вместо создания дублирующих свойств.
   - Полный запрет на `.razor.css` (за исключением QuickGrid и Markdown), инлайн `style="..."`, блоки `<style>` и TailwindCSS.
   - Полный запрет `!important` (за исключением 3 легитимных случаев: `prefers-reduced-motion`, `[hidden]`, изоляция иконок).
2. **Pure CSS Grid (Строгий запрет Flexbox)**:
   - Все лейауты, карточки, панели и стеки строятся исключительно на `display: grid`.
   - Контракт геометрии верстки: строго `minmax(0, 1fr)` для гибких колонок (запрет голого `1fr`).
   - Обязательный `min-width: 0;` на всех ячейках сетки с текстовым переполнением (`text-overflow: ellipsis`).
3. **M3 Expressive (M3X) визуальный стиль и токенизация**:
   - Выразительные радиусы: карточки `24px` (`--md-sys-shape-expressive-card`), кнопки `20px`, чипы/pills `9999px`, модальные окна `32px`.
   - Пружинная физика анимаций: `cubic-bezier(0.175, 0.885, 0.32, 1.275)`, тактильный отклик `:active scale(0.96)`.
   - Цвета и темы: только M3-токены `var(--md-sys-color-*)`, WCAG 2.1 AA в Light и Dark темах (`:root[data-theme="dark"]`, `:root[data-theme="light"]`).
4. **CSS Container Queries (`@container`)**:
   - Микро-адаптивность виджетов, модалок и карточек через `@container` вместо глобальных медиа-запросов окна браузера.
5. **Эргономика тач-интерфейсов**:
   - POS: `>= 48x48px`, Kiosk: `>= 56x56px`, Checkout: `>= 64px`, отступы `>= 12px`.
   - Отсутствие горизонтального скролла (`scrollWidth <= clientWidth`) во всех разрешениях (`320px` - `1920px`).

---

## 3. Verify (machine gates, all must pass)

```powershell
npm run validate:syntax
npm run lint:css
npm run build                        # валидация синтаксиса CSS, сборка Web Components JS и генерация C#-констант
npm run generate:codegen ; npm run generate:analytics
dotnet build src/Frontend/Platform/Web.UI/Web.UI/Web.UI.csproj -c Debug
```

Gates:
- build: 0 errors, 0 new warnings;
- `manifest.json` → target page `totalViolations == 0`;
- `lint:css` → 0 stylelint errors;
- `class-constants.json` → no newly dead constants, no new orphaned classes;
- blueprints → 0 `BoundaryOverflow` и 0 `SiblingCollision`.

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
