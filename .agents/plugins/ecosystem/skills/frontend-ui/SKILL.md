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

Все стили централизованно выносятся в проект `Assets` (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) по слоёной архитектуре Cascade Layers (`@layer tokens, foundations, components, widgets, pages, layouts`) с токенами W3C DTCG в `tokens/` через мастер-манифест `styles.css`.
Слои строго соответствуют файловой структуре:
- `layouts/` (**Уровень 1 DOM**): Стили строго для `body` через `<layout [variant]>` (`body:has(layout[grid])`) и прямых макро-ориентиров (`> header`, `> nav`, `> main`, `> footer`).
- `pages/` (**Уровень 2 DOM**): Стили строго для прямых ориентиров `body`, помеченных булевым атрибутом страницы (`main[kiosk]`, `main[terminal]`, `main[orders]`, `main[about]`, `header[about]`). Внутренние стили виджетов здесь строго запрещены.
- `widgets/` (**Уровень 3 DOM**): Составные смарт-организмы бизнеса (корзина, KDS, карточки, фискальная панель, бенто-сетки).
- `components/` (**Уровни 4–6 DOM**): Атомы и защищенные примитивы UI (кнопки, чипы, инпуты, диалоги) и общие утилиты.

Компоненты независимы и инкапсулируют свои стили в целевых файлах, а повторно используемые сетки оформляются как примитивы с нулевой специфичностью `:where(...)` (0, 0, 0):

```css
:where(.grid-split) {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--md-sys-spacing-3);
}
```

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
- [ ] Запуск `npm run refresh` в `Assets` проходит без ошибок.
