---
name: frontend-css-styles
description: "Architectural standard and workflow for CSS styles (*.css) in Socratic. USE FOR: centralized styles in Assets/wwwroot/css/, Selector Consolidation (:where(...) master selectors), pure CSS Grid, M3 Window Size Classes, Container Queries (@container), M3X design tokens, Light/Dark themes, touch ergonomics, and rendered browser verification. DO NOT USE FOR: Razor template markup (use frontend-razor-markup) or C# code-behind (use frontend-csharp-interactive)."
license: MIT
---

# Frontend Skill: CSS Styling (*.css)

Краткое руководство по разработке стилей и раскладки компонентов.
Полная архитектурная спецификация: [rules/frontend-css-styles.md](../../../../rules/frontend-css-styles.md).

---

## 1. Консолидация по селекторам в Assets

Все стили централизованно выносятся в проект `Assets` (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) и компилируются через LightningCSS.
Одинаковые CSS-свойства группируются в мастер-селекторы `:where(...)` с нулевой специфичностью (0, 0, 0):

```css
:where(.panel-header, .card-header, .dialog-header) {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--md-sys-spacing-3);
}
```

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

- [ ] Все стили добавлены в `Assets/wwwroot/css/` через консолидацию селекторов `:where(...)`.
- [ ] Раскладка построена на CSS Grid с соблюдением `minmax(0, 1fr)` и `min-width: 0`.
- [ ] Все цвета и формы используют токены Material Design 3 (полная поддержка темной и светлой тем).
- [ ] Интерактивные элементы имеют touch target не менее 48x48px.
- [ ] Запуск `npm run refresh` в `Assets` проходит без ошибок.
