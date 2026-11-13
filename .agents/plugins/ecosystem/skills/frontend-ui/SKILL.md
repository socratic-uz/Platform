---
name: frontend-ui
description: "Architectural standard and workflow for CSS styles (*.css) in Socratic. USE FOR: centralized styles in Assets/wwwroot/css/, layered architecture — Cascade Layers (@layer tokens, foundations, components, widgets, pages, layouts), pure CSS Grid, M3 Window Size Classes, Container Queries (@container), M3X design tokens, Light/Dark themes, touch ergonomics, and rendered browser verification. DO NOT USE FOR: Razor template markup (use frontend-ux) or C# code-behind (use frontend-ix)."
license: MIT
---

# Frontend Skill: CSS Styling (*.css)

Краткое руководство по разработке стилей и раскладки компонентов.
Полная архитектурная спецификация: [rules/frontend-ui.md](../../../../rules/frontend-ui.md).

---

## 1. Слоёная архитектура — Cascade Layers в Assets

Все стили централизованно выносятся в проект `Assets` (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) по слоёной архитектуре Cascade Layers (`@layer tokens, foundations, components, widgets, pages, layouts`) с токенами W3C DTCG в `tokens/` через автоматически генерируемый мастер-манифест `styles.css` (`npm run generate:styles` / `scripts/generate-styles-manifest.mjs`).
В `fonts/` подключаются **только `@font-face`** вне слоёв (`Roboto` и `Montserrat` `100 900`, `Material Symbols Outlined`), а стили `md-icon` лежат в `foundations/icons.css`. Порядок селекторов внутри изолированных таблиц строго упорядочен по обходу DOM-дерева в глубину (`npm run sort:css`).
Слои делятся на **глобальные** и **изолированные по атрибуту 1-в-1 (`[attr].css`)**:
- **Глобальные**: `fonts/`, `tokens/`, `foundations/` (сброс `reset.css`, макросетка `grid.css`, эффект островка `island.css` через `body::before`/`body::after` и `body > header::after` без вложенных `:has()`, ориентиры `landmarks.css`, `header.css`, `footer.css`, `rail.css`, `drawer.css`, `buttons.css`, `dialogs.css`, `icons.css`, `typography.css`).
- **Изолированные по атрибуту 1-в-1 (`[attr].css`)**:
  - `components/`: переиспользуемые UI-примитивы (`card.css`, `avatar.css`, `paginator.css`, `smart-*.css`).
  - `widgets/`: составные бизнес-виджеты (`cart.css`, `kiosk.css`, `terminal.css`, `kanban.css` и др.).
  - `pages/`: стили конкретных страниц (`landing.css`, `map.css`, `orders.css`, `home.css` и др.).
  - `layouts/`: корневая оболочка (`app.css` для `body[app]`).
- **Стиль Material 3 без рамок и теней**: декоративные `border` и `box-shadow` запрещены (кроме нативных сбросов `<fieldset>`, `<dialog>`, форм, таблиц, цитат и видоискателя FaceID); поверхности разделяются тональными цветами (`surface-container-*`) и скруглениями.

Компоненты независимы и инкапсулируют свои стили по уникальному корневому атрибуту — каскад между слоями полностью контролируется через `@layer`.

---

## 2. Контракт растягивания (Full Width) и явного центрирования (Opt-in Centering)

- **Растягивание по умолчанию (Stretch by Default)**:
  Все страницы, секции, сетки и ориентиры по умолчанию занимают **100% доступной области**: `width: 100%; min-width: 0; min-height: 100%; margin: 0;`. Искусственные глобальные `max-width: 1200px` и `margin: 0 auto` запрещены.
- **Явное центрирование (Opt-in via Semantic Attributes)**:
  Центрирование подключается только при необходимости через семантические атрибуты первого класса:
  - `[center]` ➔ `place-content: center; place-items: center;`
  - `[center-h]` ➔ `justify-content: center; justify-items: center;`
  - `[center-v]` ➔ `align-content: center; align-items: center;`
  - `[center-self]` ➔ `justify-self: center; margin-inline: auto;`
  - `[center-text]` ➔ `text-align: center;`

---

## 2. Геометрический контракт верстки: CSS Grid

- Все лейауты, стеки, строки и карточки строятся на **CSS Grid (`display: grid`)**.
- **Правило `minmax(0, 1fr)`**: во всех гибких колонках используется `minmax(0, 1fr)` (например, `grid-template-columns: minmax(0, 1fr) auto;`) для исключения раздувания ячейки длинным содержимым.
- **Правило `min-width: 0`**: для всех ячеек с текстом и усечением (`text-overflow: ellipsis`) контейнер снабжается `min-width: 0;`.
- **Готовые сетки-примитивы**: `.grid-center`, `.grid-split`, `.grid-flow-col`, `.m3-stack`.

---

## 3. Токены Material Design 3 и Адаптивность

- **Дизайн-токены**: цвета задаются переменными `var(--md-sys-color-*)`, радиусы скругления — `var(--md-sys-shape-corner-*)`, отступы — `var(--md-sys-spacing-*)`.
- **Контейнерные запросы (Container Queries)**: адаптивность карточек, диалогов и виджетов реализуется через `@container` от размера родительского блока.
- **Стандартные брейкпоинты M3**:
  - Compact: `< 600px`
  - Medium: `600px – 839.98px`
  - Expanded: `840px – 1199.98px`
  - Large: `1200px – 1599.98px`
  - Extra-Large: `≥ 1600px`

---

## 4. Чеклист проверки (DoD)

- [ ] Стили добавлены в соответствующий слой `Assets/wwwroot/css/` по слоёной архитектуре Cascade Layers (`@layer`).
- [ ] Раскладка построена на CSS Grid с соблюдением `minmax(0, 1fr)` и `min-width: 0`.
- [ ] Все цвета и формы используют токены Material Design 3 (полная поддержка темной и светлой тем).
- [ ] Интерактивные элементы имеют touch target не менее 48x48px.
- [ ] Пройдена валидация порядка DOM: `npm run sort:css:check`.
- [ ] Манифест актуален: `npm run generate:styles`.
- [ ] Запуск `npm run refresh` в `Assets` проходит без ошибок.
