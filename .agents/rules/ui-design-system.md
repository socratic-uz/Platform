# Socratic UI & Design System Architecture Guidelines

## 1. Общий принцип: Максимальная производительность UI
Интерфейс должен быть максимально быстрым, отзывчивым и легким.

### Приоритет технологий:
1. Native UI (Blazor Hybrid / .NET MAUI)
2. Blazor Razor Components
3. Web Components (`@material/web`)
4. Семантические HTML элементы

---

## 2. Иерархия UI Компонентов (Строгий Приоритет)
При добавлении любых UI-элементов строго соблюдать приоритет:

1. **Готовый Razor Component из библиотеки `Material.Web`** (`src/Frontend/Platform/Shared/DesignSystem/Material.Web/`):
   - `<Button Type="filled|outlined|text|elevated|tonal">`
   - `<Icon Name="icon_name" />`
   - `<TextField>`, `<Checkbox>`, `<Switch>`, `<Dialog>`
2. **Если подходящего Razor компонента нет**:
   - Использовать качественный голый веб-компонент `@material/web`: `<md-filled-button>`, `<md-outlined-text-field>`, `<md-checkbox>`, `<md-switch>`, `<md-chip-set>`, `<md-dialog>`, `<md-tabs>`.
3. **Если и этого нет**:
   - Использовать стандартные семантические HTML элементы (`<button>`, `<input>`, `<article>`, `<section>`, `<nav>`).
- **Не создавать собственные велосипеды и самодельные компоненты без необходимости.**
- UI обязан соответствовать принципам Material Design 3, WCAG Accessibility и семантическому HTML.

---

## 3. Web UI: Blazor Web App (Global Auto Rendering)
- **Режим**: **Global Auto Rendering** (`InteractiveAuto`).
- **Цели**:
  - Быстрый первый ответ сервера (SSR Prerendering).
  - Мгновенная интерактивность после загрузки клиентского WebAssembly.
  - Минимальный JavaScript и нулевые сторонние тяжелые библиотеки.
- **State Persistence**:
  - Обязательно использовать `PersistentComponentState` в `OnInitializedAsync`, чтобы предотвратить повторные сетевые запросы при переходе от SSR к WASM.
- **Zero-Flicker (Anti-FOUC)**:
  - Синхронное определение темы в `<head>` `App.razor`.
  - Маскирование незагруженных веб-компонентов `:not(:defined) { opacity: 0; }`.
  - Защита от мигания лигатур шрифтов (`font-display: block;`, `md-icon:not(:defined) { font-size: 0; }`).
  - Обязательное использование `@key` в `@foreach` для оптимизации render tree diff.

---

## 4. Native UI: Blazor Hybrid / .NET MAUI (POS & Kiosk)
- Сборка должна использовать Native AOT и агрессивный тримминг с минимальным runtime overhead.
- Минимизировать reflection, XAML runtime magic и dynamic binding.
- Гарантировать touch-friendly зоны взаимодействия: минимум 48x48px для кассовых терминалов и киосков.

---

## 5. Frontend Performance Standards
- **Virtualize**: Обязательное использование `<Virtualize>` для длинных каталогов и списков заказов.
- **Streaming Rendering**: Использование streaming rendering для отложенной загрузки тяжелых блоков.
- **Дизайн-токены (Vanilla CSS, NO Tailwind, NO .razor.css)**:
  - Запрещен TailwindCSS.
  - Полный отказ от компонентной CSS-изоляции: все стили централизованно хранятся в проекте **`Assets`** (исключения: строго `QuickGrid` и `Markdown`).
  - Все цвета задаются через CSS Custom Properties Material 3:
    - Surface: `var(--md-sys-color-surface)`, `var(--md-sys-color-surface-container)`
    - Primary: `var(--md-sys-color-primary)`, `var(--md-sys-color-on-primary)`
    - Outline: `var(--md-sys-color-outline)`, `var(--md-sys-color-outline-variant)`

---

## 6. 📐 Строгий стандарт верстки: Исключительно CSS Grid (NO Flexbox)
> **КАТЕГОРИЧЕСКИЙ ЗАПРЕТ**: Использование `display: flex` строго запрещено во всех компонентах и стилях Socratic.
> Все лейауты, контейнеры, строки, колонки, списки и центрирование реализуются **только через CSS Grid (`display: grid`)**.

### Почему CSS Grid вместо Flexbox:
1. **Детерминированность**: Нет неожиданного сжатия элементов (`flex-shrink`), выхода за пределы экрана (`min-width: 0` бага) и плавающих багов переноса.
2. **Двухосевой контроль**: Строгое позиционирование как по горизонтали, так и по вертикали.
3. **Меньше CSS**: `place-items: center;` вместо связки `justify-content` + `align-items`, предсказуемый `grid-template-columns`.

### Паттерны замены Flexbox на Grid (с защитой от переполнения):
- **Вертикальный стек (колонка)**:
  ```css
  /* ВМЕСТО: display: flex; flex-direction: column; gap: 12px; */
  display: grid;
  gap: 12px;
  ```
- **Горизонтальный ряд (фиксированный компактный ряд)**:
  ```css
  /* Только если число элементов мало и фиксировано (<= 280px суммарно) */
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: max-content;
  gap: 8px;
  align-items: center;
  ```
- **Горизонтальный скроллируемый стрип (чипы, табы, длинный ряд кнопок)**:
  ```css
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: max-content;
  gap: 8px;
  overflow-x: auto;
  scrollbar-width: none;
  width: 100%;
  ```
- **Разделение лево / право (Space-Between) с защитой от blowout**:
  ```css
  /* ВМЕСТО: display: flex; justify-content: space-between; */
  /* minmax(0, 1fr) ОБЯЗАТЕЛЕН, чтобы длинный заголовок не распирал сетку */
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: 8px;
  ```
- **Центрирование**:
  ```css
  /* ВМЕСТО: display: flex; justify-content: center; align-items: center; */
  display: grid;
  place-items: center;
  ```
- **Адаптивная сетка карточек**:
  ```css
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 280px), 1fr));
  gap: 16px;
  ```

> Подробный геометрический контракт, правила `min-width: 0`, Container Queries и поведение диалогов: [rules/ui-layout-and-geometry.md](file:///c:/Users/owner/source/repos/Socratic/.agents/rules/ui-layout-and-geometry.md).

---

## 7. 📁 Архитектура файлов компонентов (1 или 2 файла, полный отказ от `.razor.css`)
> **ОТКАЗ ОТ CSS-ИЗОЛЯЦИИ**: Проект полностью отказался от компонентной изоляции стилей (`.razor.css`). Все стили централизованно хранятся и развиваются исключительно в проекте **`Assets`** (`src/Frontend/Platform/Shared/DesignSystem/Assets/` -> W3C модули в `css/`, компилируемые LightningCSS в `material.bundle.min.css`).
> **ИСКЛЮЧЕНИЯ**: Строго **`QuickGrid`** и **`Markdown`** (только этим двум компонентам разрешено иметь локальные `.razor.css`).

Количество файлов выбирается строго по C#-сложности компонента:

### 1 Файл (`.razor` с блоком `@code`)
- **Когда использовать**:
  - Атомарные, презентационные («dumb») и компоненты средней сложности (< 150 строк разметки и C#-логики).
  - Минимальная inline-логика (эмит событий, переключение флага, базовые параметры).
  - Стили строго берутся из сгенерированных констант классов `Classes.*` (`Classes.g.cs`) и глобальных Grid-примитивов (`.grid-center`, `.grid-split`, `.m3-stack`) из проекта `Assets`.
- **Примеры**: бейджи статусов, кнопки-обертки, карточки товаров, тулбары, диалоги с базовыми полями ввода.

### 2 Файла (`.razor` + `.razor.cs`)
- **Когда использовать**:
  - Крупные бизнес-фичи, страницы, смарт-компоненты и оркестраторы (Feature Slices).
  - Наличие сложного жизненного цикла (`OnInitializedAsync`, `PersistentComponentState`, `IAsyncDisposable`).
  - Множество DI-инъекций, подписки на события/шину, валидация форм, JS-интероп, сложная математика координат/графиков.
  - Объемный C#-код (> 50-80 строк).
- **Примеры**: `MapPage`, `SmartSchedulePicker`, `OrderGrid`, `TerminalWorkspace`.

### 3 Файла с `.razor.css`
- **КАТЕГОРИЧЕСКИ ЗАПРЕЩЕНЫ** во всем репозитории, за исключением двух официально утвержденных компонентов: **`QuickGrid`** и **`Markdown`**. Любые новые `.razor.css` файлы подлежат удалению при код-ревью с переносом правил в W3C-модули `Assets`.

---

## 8. ♻️ Переиспользование CSS и устранение избыточности (CSS Hygiene)

### 1. Централизованные CSS Grid примитивы
Не дублируйте одинаковые декларации Grid по 10 раз в разных `.razor.css`. Используйте общие утилитные классы из `touch.css` / глобальных стилей:
- `.grid-center` — центрирование элемента (`display: grid; place-items: center;`).
- `.grid-split` — разделение заголовка и действий (`display: grid; grid-template-columns: 1fr auto; align-items: center;`).
- `.grid-flow-col` — ряд элементов равной/авто высоты (`display: grid; grid-auto-flow: column; align-items: center; gap: ...`).
- `.grid-stack` — вертикальный стек элементов (`display: grid; gap: ...`).

### 2. Запрет мертвого и зомби-CSS
- В проекте **категорически запрещены** остатки чужих CSS-фреймворков (Bootstrap, Tailwind, Bulma).
- Любые классы вроде `.btn-primary`, `.form-control`, `.row`, `.col-*`, переменные `--bs-*` подлежат немедленному удалению при обнаружении.

### 3. Строгая токенизация
- Никаких жестко закодированных HEX/RGB цветов (`#1e293b`, `rgba(0,0,0,0.1)`).
- Все цвета, скругления и тени должны ссылаться исключительно на токены Material Design 3 (`var(--md-sys-color-*)`, `var(--md-sys-shape-*)`).

---

## 9. 🏛️ W3C CSS Modules & Single-Declaration Consolidation

### 1. Архитектура папки `css/`
Все стили экосистемы (`Material.Web/wwwroot/css/` и `Layout/wwwroot/css/`) строго соответствуют модульной спецификации W3C:
- `foundations/` — normalize, сброс стилей браузера.
- `tokens/` — W3C Custom Properties (размеры, отступы, тени, переходы).
- `themes/` — темы (dark, light, expressive).
- `box-model/` — sizing, margin, padding, border, outline.
- `layout/` — display, grid, alignment, multi-column, table, showcase.
- `positioning/` — position, stacking, anchor.
- `overflow/` — overflow, scrolling, scrollbars.
- `typography/` — font, text-layout, text-wrapping.
- `effects/`, `motion/`, `interaction/`, `content/`, `containment/`.
- `material.css` — мастер-манифест импортов.
- `material.bundle.min.css` — единый скомпилированный бандл (компилятор LightningCSS `bundle-css.mjs`).

### 2. Философия единственной декларации (Single-Declaration Consolidation)
- **Каждая CSS-декларация (свойство + значение) должна встречаться ровно один раз** во всей библиотеке стилей.
- Селекторы, разделяющие одно и то же свойство, группируются в мастер-селектор `:where(...)`:
  ```css
  /* Пример из layout/grid.css: одно общее объявление для всех контейнеров */
  :where(.m3-grid, .m3-card, .card-grid, .catalog-sidebar) {
      display: grid;
  }
  ```
- **Запрет дублирования**: категорически запрещено объявлять `display: grid;` или `box-sizing: border-box;` отдельно для каждого класса.

### 3. Методы и правила минимизации CSS-кода (Minification & Deduplication Rules)

#### Правило 1: Дополнение мастер-селекторов (Selector Append Pattern)
* При создании нового компонента или элемента **запрещено** создавать новый блок стилей с уже существующим в системе свойством.
* Разработчик или агент обязан найти соответствующую группу в W3C-модуле и добавить новый селектор в `:where(...)`:
  ```css
  /* Пример в css/box-model/sizing.css */
  :where(
      .assistant-message-icon,
      .assistant-search-icon,
      .citation svg,
      .new-feature-icon /* <- просто дописываем новый селектор */
  ) {
      width: 1.5rem;
      height: 1.5rem;
  }
  ```

#### Правило 2: Shorthand-консолидация (Минимизация синтаксиса)
* **Центрирование**: строго `place-items: center;` в `css/layout/alignment.css` (вместо `align-items: center; justify-items: center;` или flex `justify-content + align-items`).
* **Позиционирование оверлеев**: строго `inset: 0;` (вместо `top: 0; right: 0; bottom: 0; left: 0;`).
* **Логические оси**: `padding-inline: 1rem;` и `margin-inline: auto;` вместо раздельных свойств `-left` и `-right`.
* **Легковесное обрезание**: `overflow: clip;` вместо тяжелого `overflow: hidden;` там, где не требуется создание нового контекста прокрутки.

#### Правило 3: Строгая доменная чистота файлов (W3C Domain Separation)
Каждый файл отвечает строго за свой семантический домен W3C спецификации:
* В `css/box-model/sizing.css`: **только** `width`, `height`, `min-width`, `min-height`, `max-width`, `max-height`.
* В `css/layout/grid.css`: **только** `display: grid`, `grid-template-*`, `grid-auto-*`, `gap`.
* В `css/box-model/padding.css`: **только** `padding`, `padding-inline`, `padding-block`.
* В `css/box-model/margin.css`: **только** `margin`, `margin-inline`, `margin-block`.
* В `css/box-model/border.css`: **только** `border`, `border-radius`, `border-color`.
* В `css/typography/text-wrapping.css`: **только** `overflow: hidden; text-overflow: ellipsis; white-space: nowrap;` и line-clamp.
* *Смешение свойств из разных доменов в одном файле строго запрещено.*

#### Правило 4: Управление специфичностью через `:where(...)` (Zero Specificity Master Rules)
* Все консолидированные мастер-селекторы группируются строго через `:where(.c1, .c2, ...)`, обеспечивая **нулевую специфичность `(0, 0, 0)`**.
* **Почему не `:is(...)`**: по стандарту W3C специфичность `:is()` равна наивысшей специфичности среди аргументов (например, `md-dialog [slot="headline"]` имеет `0,2,1`, что ломало адаптивность соседних классов). Псевдокласс `:where()` гарантирует идеальную нулевую специфичность.
* Любой медиа-запрос (`@media`), класс темы (`[data-theme="light"]`) или модификатор компонента (`0, 1, 0`) побеждает базовое правило автоматически **без использования `!important`**.
* **Запрет вложенности `:where(:where(...))` (Flattening)**: внутри мастер-селекторов `:where(...)` все классы перечисляются плоским списком. Вложенные скобки перед запятыми запрещены.
* **Изоляция псевдоэлементов (W3C Selectors Level 4)**: по спецификации W3C псевдоэлементы (`::before`, `::after`, `::-webkit-scrollbar`) не поддерживаются внутри `:where(...)`. Поэтому базовые селекторы оборачиваются в `:where(...)`, а псевдоэлементы выносятся наружу через запятую:
  ```css
  :where(
      .m3-card,
      .m3-dialog
  ),
  .m3-card::before,
  .m3-dialog::after {
      position: absolute;
  }
  ```

#### Правило 5: Строгий запрет `!important` и 3 легитимных исключения
В библиотеке стилей категорически запрещено использовать `!important`. Вся архитектура спроектирована так, чтобы нулевая специфичность `:where(...)` устраняла необходимость в `!important`.
* **Разрешены строго 3 исключения по стандартам W3C и WCAG**:
  1. **WCAG 2.1 AA Reflow & Motion**: `@media (prefers-reduced-motion: reduce)` — принудительное обнуление длительности анимаций (`animation-duration: 0.01ms !important; transition-duration: 0.01ms !important;`).
  2. **HTML5 Нативное скрытие**: `[hidden], md-dialog:not([open]), md-menu:not([open]) { display: none !important; }` и отключение нативного скроллбара (`::-webkit-scrollbar { display: none !important; }`).
  3. **Изоляция иконочных шрифтов**: `font-family: 'Material Symbols Outlined' !important;` (защита системных глифов от перезаписи пользовательскими шрифтами).
* Любое другое использование `!important` для размеров, сеток, цветов, отступов или позиционирования считается архитектурным браком.

#### Правило 6: Синергия с алгоритмами сжатия Brotli и Gzip
* Благодаря повторяющейся структуре мастер-правил `:where(...)` и стандартизированных токенов `var(--md-sys-*)`, словарь компрессора Brotli сжимает бандл до 85–90% (бандл 130 КБ сжимается до ~16–18 КБ при передаче по сети).

### 4. Стандартизация селекторов на семантические M3 примитивы (Semantic M3 Primitives)
- **Запрет ad-hoc селекторов**: категорически запрещено придумывать разрозненные классы-однодневки для каждого нового экрана (такие как `.demo-card`, `.selection-panel`, `.progress-item-box`, `.card-avatar`, `.card-actions-grid`).
- **Семантический M3 алфавит**: все компоненты обязаны строиться из канонических M3 примитивов:
  - **Карточки и поверхности**: `.m3-card`, `.m3-card-outlined`, `.m3-card-elevated`, `.m3-surface-container-high`.
  - **Слоты компонентов**: `.m3-card-header`, `.m3-card-actions`, `.m3-card-title`, `.m3-card-subtitle`, `.m3-avatar`, `.m3-price-badge`.
  - **Стеки и метки**: `.m3-stack`, `.m3-label`, `.m3-label-sm`, `.m3-col-span-full`.
- **Запрет Bootstrap-подобных инлайн-стилей**:
  - Строгий запрет передачи параметров сетки в разметку через инлайн-стили (например, `style="--col-min: 280px"`).
  - Все сетки определяются как чистые семантические классы в `layout/grid.css` (`.m3-grid-buttons`, `.m3-grid-fabs`, `.m3-grid-chips`, `.m3-grid-cards-split`).
- **Доменные оболочки**: для страниц и модулей допускается единственный доменный префикс (`.catalog-shell`, `.catalog-sidebar`, `.catalog-header`, `.pos-shell`, `.kiosk-shell`).

### 5. Запрет стилей в разметке и ad-hoc файлов
- **Строгий запрет `<style>` в HTML/Razor**: HTML и Razor-разметка предназначены для демонстрации, валидации и использования стилей дизайн-системы, а не для создания новых правил.
- **Строгий запрет временных файлов** (`components.css`, `demo.css`, `custom.css`): любые новые утилиты или сетки стендов оформляются строго как W3C-модули в `css/` (например, `css/layout/showcase.css`) и компилируются в мастер-бандл `material.bundle.min.css`.
- **Компоненты Material Design 3 (`@material/web`)**: стилизуются исключительно через MD3 токены (`var(--md-sys-*)`) и правила в `css/layout/display.css`, без внешних CSS-костылей.



