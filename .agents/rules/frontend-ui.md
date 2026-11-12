# Socratic Design System — Архитектура стилей (UI & Presentation)

## 1. 🏛️ Фундаментальные принципы архитектуры стилей Socratic

Вся визуальная презентация экосистемы Socratic централизованно сосредоточена в проекте **`Assets`** (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) и строится на **6 ключевых столпах**:

### 1.1. Централизованная нативная платформа (Native W3C Platform)
- **Единый центр стилизации**: все стили живут в `Assets/wwwroot/css/` с точкой входа [`styles.css`](file:///d:/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/styles.css).
- **Динамическая сборка манифеста (`generate-styles-manifest.mjs`)**: `styles.css` генерируется автоматически скриптом `scripts/generate-styles-manifest.mjs` (при `dotnet build Assets.csproj` и `npm run generate:styles`) через рекурсивное сканирование слоев.
- **Шрифтовая архитектура вне слоев (Unlayered Variable Fonts)**: правила `@font-face` подключаются в самом верху `styles.css` **вне `@layer`**, что обеспечивает наивысший приоритет браузерного парсинга, мгновенное обнаружение глифов и исключение FOUT для лигатур иконок (`Material Symbols Outlined`). Используются исключительно вариативные шрифты (`Roboto` и `Montserrat` с диапазоном весов `100 900`, а также вариативный `Material Symbols Outlined`); статические TTF-файлы полностью ликвидированы.
- **Строгий скоуп `foundations/grid.css` (Сетка документа и ориентиров — CSS Grid ONLY)**: файл `foundations/grid.css` содержит исключительно свойства CSS Grid для макросетки `body` и прямых ориентиров (`body > header`, `body > aside`, `body > main`, `body > footer`). Вся внутренняя стилизация компонентов, кнопок или заголовков выносится в соответствующие виджеты (`widgets/`), а визуальные свойства ориентиров изолированы в `foundations/landmarks.css`.
- **Разделение ответственности**: разметка компонентов (`*.razor`) свободна от инлайн-стилей, тегов `<style>` и классов `class=@...`. Стилизация детерминирована на уровне атрибутов первого уровня (`[card]`, `main[kiosk]`, `aside[cart]`) и семантической иерархии тегов HTML5.

### 1.2. Слоёная архитектура — Cascade Layers (`@layer`) и структура директорий
Иерархия каскада построена на нативных каскадных слоях **Cascade Layers (`@layer`)** в начале `styles.css`:
```css
@layer tokens, foundations, components, widgets, pages;
```
- **Приоритет слоев**: нарастает слева направо (от абстрактного/базового к конкретному контексту экрана):
  `tokens` (1) ➔ `foundations` (2) ➔ `components` (3) ➔ `widgets` (4) ➔ `pages` (5).
- **Строгое соответствие слоев и структуры каталогов (Clean Layer Isolation)**:
  1. `tokens` (`tokens/`): дизайн-токены W3C DTCG и системная палитра Material Design 3 Expressive (M3X).
  2. `foundations` (`foundations/`): сброс стилей браузера, семантические дефолты HTML5, типографика, анимации, контракт центрирования (`[center*]`), базовая стилизация ориентиров в `landmarks.css` и чистая геометрия сетки в `foundations/grid.css`.
  3. `components` (`components/`): атомарные и молекулярные примитивы UI Material 3 (`buttons`, `cards`, `forms`, `dialogs`, `chips`).
  4. `widgets` (`widgets/`): составные отраслевые бизнес-организмы (`cart`, `fiscal`, `map`, `chat`, `detector`, `landing`, `analytics`, `kiosk`, `orders`, `payment`), а также внутренние организмы ориентиров (`rail.css`, `drawer.css`, `header.css`, `footer.css`).
  5. `pages` (`pages/`): стили **строго для конкретных страниц по булевым атрибутам** (`main[kiosk]`, `main[terminal]`, `main[orders]`, `main[about]`, `main[home]`). Вся внутренняя структура страниц, карточки, панели и списки выносятся в `widgets/`.
  6. `layouts` (`layouts/`): Blazor AppLayout шеллы (`app.css`, селектор `body[app]`).

- **Чистая геометрия сетки документа и ориентиров (`foundations/grid.css` — CSS Grid Only)**:
  - **Макросетка `body`**: единая 3-колоночная сетка задаётся ровно 1 раз: `grid-template-columns: auto minmax(0, 1fr) auto;`, а адаптивность и режимы переключаются исключительно через `grid-template-areas`.
  - **Сетка ориентиров 1-го уровня под `body` (ТОЛЬКО свойства CSS Grid)**:
    - Шапка: `grid-area: header; grid-template-columns: minmax(0, max-content) minmax(0, 1fr) minmax(0, max-content); gap: ...`
    - Левый ориентир: `body > aside:has(~ main) { grid-area: left; gap: 0; }`
    - Рабочая область: `body > main { grid-area: main; gap: ... }`
    - Правый ориентир: `body > main ~ aside { grid-area: right; gap: 0; }`
    - Подвал: `body > footer { grid-area: footer; }`
  - **Внутреннее визуальное наполнение ориентиров вынесено в `widgets/`**:
    - `widgets/rail.css`: навигационный рельс 76px на Desktop и нижний бар 60px на Mobile (ссылки, иконки, бейджи, аватар).
    - `widgets/drawer.css`: модальные шторки `md-dialog`, snap-состояния (half/full), скрим `#sheet-scrim`, GPU-анимации и кнопка FAB.
    - `widgets/header.css`: наполнение шапки (брендинг, заголовок, глобальный поиск, аватар, гамбургер).
    - `widgets/footer.css`: наполнение подвала (колонки навигации, заголовки секций `h4`, списки ссылок, копирайт).
       - **Эргономическая триада триггеров на мобильных устройствах (< 840px)**:
           - ☰ **Левый верхний угол `header`** ➔ `Вспл. лев.` (Главное меню, AI-чат, Документация) — на всю высоту `100dvh`.
           - 👤 **Правый верхний угол `header` (Аватар)** ➔ `Вспл. ниж.` (Профиль пользователя, поиск карты) — снизу экрана (`bottom: 0`).
           - ⚡ **Правый нижний угол `main` (FAB)** ➔ `Вспл. прав.` (Корзина, Чек, Детектор, QR, Кабинет, Сводка, Конструктор QR, Конструктор зала) — на всю высоту `100dvh`.
         - **Архитектура скрима (`#sheet-scrim`) и изоляция контекстов наложения (Stacking Contexts)**:
           - Полноэкранный скрим затемнения (`#sheet-scrim`) монтируется **внутри активного `<aside>` непосредственно перед `<md-dialog>`** (`targetParent.insertBefore(scrim, owner)`), исключая попадание диалога под оверлей из-за изоляции контекстов CSS Grid (`position: sticky`).
           - Уровни наложения: активный `<aside>` с открытым диалогом получает `z-index: 1000` (`body > aside:has(> md-dialog[open]:not([map-controls]))`), скрим `#sheet-scrim` — `z-index: 1000` (`inset: 0`), а сам `<md-dialog>` возвышается на `z-index: 1001`. Нижний бар мобилки — `z-index: 90`.
           - **Где скрим ЕСТЬ**: для всех всплывающих панелей на Mobile и Desktop (`Вспл. лев.`, `Вспл. прав.`, `Вспл. ниж.`).
           - **Где скрима НЕТ**: для встроенных панелей на Desktop (`[docked]`) и контролов карты (`[map-controls]`), сохраняя интерактивность рабочей области.
           - **Взаимное исключение (Mutual Exclusion)**: одновременно может быть открыта только **одна** всплывающая панель (открытие новой автоматически закрывает предыдущую).
         - **Чистые имена и GPU-анимации**:
           - Полный отказ от префикса `socratic-` в именах keyframes, ID и атрибутов: `#sheet-scrim`, `data-aside-sheet`, `data-closing`.
           - Аппаратные плавные анимации выдвижения на CSS transforms: `@keyframes slide-in-right`, `slide-out-right`, `slide-in-left`, `slide-out-left`, `slide-in-bottom`, `slide-out-bottom`.

### 1.3. Контракт растягивания на всю область (Full Width by Default) и центрирования (Opt-in Centering)
- **Растягивание по умолчанию (Stretch by Default)**:
  - Все макро-контейнеры, ориентиры (`body > header`, `body > main`, `body > footer`), страницы, сетки и секции по умолчанию стремятся занять **100% доступной области**: `width: 100%; min-width: 0; min-height: 100%; margin: 0;`.
  - Запрещены искусственные глобальные ограничения `max-width: 1200px` и авто-центрирование `margin: 0 auto` на уровне базовых страниц и ориентиров. Интерфейс естественно масштабируется от смартфонов до широкоформатных экранов, POS и 4K киосков.
- **Явное центрирование по требованию (Opt-in Centering via Semantic Attributes)**:
  Центрирование применяется осознанно только там, где это функционально необходимо (модальные карточки входа, сплэш-экраны, пустые состояния), с помощью семантических атрибутов первого класса:
  - `[center]` ➔ `place-content: center; place-items: center;`
  - `[center-h]` ➔ `justify-content: center; justify-items: center;`
  - `[center-v]` ➔ `align-content: center; align-items: center;`
  - `[center-self]` ➔ `justify-self: center; margin-inline: auto;`
  - `[center-text]` ➔ `text-align: center;`

### 1.4. Регламент разграничения: Components vs Widgets
Чтобы исключить смешение понятий, действует строгое правило классификации:

| Критерий | `components/` (Компоненты) | `widgets/` (Виджеты) |
|---|---|---|
| **Определение** | Атомарные и базовые молекулярные примитивы дизайн-системы | Составные отраслевые организмы бизнеса (HoReCa / POS / Retail) |
| **Доменная привязка** | **Нейтральны**: универсальны для любого приложения в мире без привязки к Socratic | **Специфичны**: решают конкретную задачу торговли, платежей или зала |
| **Состав** | Неделимый примитив (кнопка, чип, инпут, модалка, таблица) | Композиция нескольких компонентов (в корзине есть кнопки, чипы, карточки) |
| **Примеры в кодовой базе** | `card.css`, `avatar.css`, `color-picker.css`, `file-picker.css`, `picker-trigger.css`, `skeleton.css`, `smart-grid.css`, `paginator.css` | `cart.css`, `fiscal.css`, `kanban.css`, `merchant-settings.css`, `faceid-enrollment.css`, `qr-customizer.css`, `settings.css` |

### 1.5. Иерархический порядок стилей по DOM-дереву (DOM-Hierarchical Pre-Order Sorting)
Все таблицы стилей дизайн-системы упорядочиваются строго в соответствии со сквозным обходом DOM-дерева в глубину (Depth-First Search / Pre-Order):
1. **Корень контейнера / ориентира**: `main[kiosk]`, `header[orders]`, `aside[cart]`, `body:has(layout[...])`.
2. **Семантические потомки по цепочке `>`**: свойства родителя объявляются строго перед его детьми (`root` $\rightarrow$ `root > child` $\rightarrow$ `root > child > grandchild`).
3. **Состояния интерактивности**: базовый селектор всегда предшествует своим псевдоклассам (`article` $\rightarrow$ `article:hover`, `button` $\rightarrow$ `button:focus-visible`).
4. **Адаптивные блоки (`@media`)**: выносятся в конец файла/слоя и упорядочиваются по `max-width` по убыванию ($1024\text{px} \rightarrow 840\text{px} \rightarrow 600\text{px}$), а правила внутри каждого `@media` также строго подчиняются DOM-порядку.
5. **Автоматизация**: проверка и форматирование порядка осуществляется командой `npm run sort:css` (проверка: `npm run sort:css:check`).

### 1.6. Принцип строгого паритета 1-в-1 (Razor Markup ⟷ CSS Stylesheet Parity)
Каждый файл разметки `*.razor` (страница, виджет, компонент) подчиняется закону взаимно однозначного соответствия:
- **Атрибут строго на 1-м корневом уровне DOM**: компонент разметки маркируется уникальным атрибутом на своём корневом контейнере (`<header kiosk>`, `<main kiosk>`, `<aside cart>`, `<main wishlist>`).
- **Собственный файл стилей**: у каждой сущности есть свой собственный одноимённый `.css` файл стилей, опирающийся строго на её атрибут:
  - `Kiosk.razor` $\iff$ `pages/kiosk.css` (`[kiosk]`)
  - `WishList.razor` $\iff$ `pages/wishlist.css` (`[wishlist]`)
  - `Home.razor` $\iff$ `pages/home.css` (`[home]`)
  - `Qr.razor` $\iff$ `pages/qr.css` (`[qr]`)
  - `QrDesignerPage.razor` $\iff$ `pages/qr-designer.css` (`[qr-designer]`)
- **Запрет смешивания контекстов**: категорически запрещено помещать стили разных страниц или разнородных сущностей в один `.css` файл (например, стили `wishlist` в `kiosk.css` или `qr` в `qr-designer.css`).

### 1.7. Онтология токенов W3C DTCG & Material Design 3 Expressive
- Слой `tokens/` организован строго по 10 каноническим типам спецификации W3C Design Tokens Community Group:
  1. `border.css` — радиусы скруглений и толщины рамок ($type: border)
  2. `colors.css` — системная палитра M3X и режимы тем Dark/Light ($type: color)
  3. `motion.css` — длительности переходов и сплайн-кривые анимаций ($type: duration / transition)
  4. `outline.css` — стили фокусов и контуров ($type: outline)
  5. `overflow.css` — режимы скроллинга и усечения ($type: overflow)
  6. `positioning.css` — z-index уровни и слои наложения ($type: positioning)
  7. `shadow.css` — шкала теней M3 и эффекты стекла ($type: shadow / elevation)
  8. `sizing.css` — брейкпоинты, тач-таргеты и пропорции ($type: dimension / sizing)
  9. `spacing.css` — модульная сетка отступов ($type: dimension / spacing)
  10. `typography.css` — шрифтовая шкала Material Typescale ($type: typography)
- **Сквозная типизация в C#**: иконки, ресурсы и локализация компилируются генераторами в строго типизированные структуры (`Icons.g.cs`, `Assets.g.cs`, `Loc.g.cs`), обеспечивая 100% проверку компилятором без строковой магии. Генераторы классов (`Classes.g.cs`) ликвидированы в пользу Pure Attribute-Driven Classless.

### 1.8. Детерминированная геометрия: Pure CSS Grid & Container Queries
- **Двумерная математическая модель**: все сетки, колонки, ряды и стеки строятся исключительно на `display: grid`.
- **UI Geometry Contract**:
  - Каждая гибкая колонка защищена `minmax(0, 1fr)` (исключает переполнение контейнера).
  - Ячейки с усечением текста (`text-overflow: ellipsis`) снабжаются `min-width: 0;`.
- **CSS Container Queries (`@container`)**: Micro Layout (карточки, модальные окна, виджеты) адаптируется к ширине своего непосредственного родителя, а не экрана.
- **M3 Window Size Classes**: Macro Layout (каркас приложения, навигация, рабочие зоны) управляется адаптивной матрицей вьюпортов M3 (`Compact`, `Medium`, `Expanded`, `Large`, `ExtraLarge`).

### 1.9. Pure Attribute-Driven Classless — Трёхуровневая семантическая навигация и селекторы
Все стили компонентов, виджетов и страниц строятся строго по трёхуровневой модели без классов:

```text
1. Уровень документа (Body)
   └── body:has(layout[grid]) { ... }
   └── body:has(layout[landing]) { ... }

2. Уровень страницы (Landmarks первого уровня)
   ├── header[page] > tag1 > tag2 { ... }
   ├── nav[page] > tag1 > tag2 { ... }
   ├── main[page] > tag1 > tag2 { ... }
   └── footer[page] > tag1 > tag2 { ... }

3. Уровень виджетов (Root-tag первого уровня внутри виджета)
   └── root-tag[widget] > child-tag > child-child-tag { ... }
```

- **1. Уровень документа (`body`)**:
  - Единая универсальная 3-колоночная макросетка (`grid-template-columns: auto minmax(0, 1fr) auto;`). Адаптивность переключается через `grid-template-areas`. Механизм служебного тега `<layout>` упразднен.
- **2. Уровень страницы (`landmarks`)**:
  - На первом уровне DOM страницы находятся её семантические ориентиры: `header[page]`, `nav[page]`, `main[page]`, `footer[page]`.
  - Стили каждого ориентира пишутся **строго от самого landmark'а**:
    - `header[kiosk] > nav > hgroup > p` (категорически запрещено писать `body:has(main[kiosk]) header...` — `header[kiosk]` сам является ориентиром 1-го уровня).
    - `main[kiosk] > nav`, `main[kiosk] > article`, `main[kiosk] :where(md-filled-card)`.
    - `footer[kiosk] > :where(md-text-button, button)`.
- **3. Уровень виджетов (`widgets`)**:
  - Корневой тег первого уровня виджета несёт атрибут виджета:
    - `aside[cart] > section > ul > li`
    - `aside[order] > section > article`
    - `dialog[modal] > fieldset > menu`
- **Атрибут строго на первом уровне DOM-дерева в Razor-разметке**:
  - Дочерние элементы внутри контейнеров не несут атрибутов и являются чистыми HTML5-тегами (`tag1`, `tag2`, `child-tag`).
- **Нулевая специфичность**: использование `:where(...)` сохраняет вес `(0, 0, 0)`, позволяя слоям каскада (`@layer`) и токенам управлять оформлением без специфических конфликтов.

### 1.6. Модульность компонентов и нулевая специфичность примитивов (`:where(...)`)
- **Модульная изоляция**: каждый компонент (`buttons.css`, `cards.css`, `dialogs.css`, `forms.css`) независим и инкапсулирует собственные правила. Компоненты слабо связаны (Loose Coupling).
- **Сетки-примитивы (Layout Primitives)**: переиспользуемая геометрия выносится в семантические Grid-примитивы (`.grid-split`, `.grid-center`, `.m3-stack`) в слое `layouts/`.
- **Роль `:where(...)`**: псевдокласс `:where(...)` применяется исключительно для обнуления специфичности `(0, 0, 0)` базовых сбросов (CSS Reset в `foundations/`) и UI-примитивов, чтобы компоненты верхних слоев могли легко переопределять их без хаков специфичности и без `!important`. Склеивание разнородных селекторов через запятую запрещено как антипаттерн.

### 1.7. Classless-First Foundation (`foundations/base.css`)
- **Цель**: минимум CSS и классов. Нативные теги HTML5 стилизуются глобально токенами M3 в `@layer foundations`, всё через `:where(...)` → специфичность `(0, 0, 0)`.
- **Типографика глобально**: `h1–h6`, `p`, `a`, `small`, `strong`, `mark`, `code`, `kbd`, `pre`, `blockquote`, `hr`, `figure/figcaption`, `dl/dt/dd` — без классов.
- **Classless-when-classless**: богатые M3-дефолты для структурных и интерактивных тегов применяются только к элементам **без атрибута `class`** — `:where(tag:not([class]))`:
  - `<button>` ➔ M3 Filled (≥ 48px), `<button type="reset">` ➔ Outlined;
  - `<article>` ➔ карточка, `<aside>` ➔ панель, `<nav>` ➔ горизонтальный ряд, `<section>` ➔ grid-стек;
  - `<ul>/<ol>` ➔ контентный список; `<table>` ➔ M3 таблица; `<fieldset>/<legend>` ➔ outlined-группа;
  - `<label>` ➔ подпись над полем; `<label>` с checkbox/radio/switch ➔ inline-ряд ≥ 48px;
  - `<input>/<select>/<textarea>` ➔ M3 outlined field ≥ 48px; `<details>/<summary>` ➔ аккордеон; `<dialog>` ➔ M3 Basic Dialog.
- **С классом** — элемент получает только нейтральный сброс (списки без маркеров, fieldset без рамки), геометрией полностью владеет компонент.
- **Правило для новой разметки**: сначала семантический тег без класса; класс добавляется только для варианта M3 или специфичной Grid-раскладки.

### 1.8. Сенсорная эргономика (Touch-First & High-Throughput POS/Kiosk)
- Габариты интерактивных зон: POS $\ge 48\times 48\text{px}$, Kiosk $\ge 56\times 56\text{px}$, Fast Checkout $\ge 64\text{px}$, отступы между кнопками $\ge 12\text{px}$.
- `touch-action: manipulation` для устранения браузерных задержек сенсорного ввода.

---

## 2. 📐 Геометрический контракт верстки: CSS Grid

Все компоновки, выравнивания, стеки, ряды и карточки реализуются через **CSS Grid**.

### 2.1. Правило `minmax(0, 1fr)` (Защита от Blowout)
- В гибких колонках или рядах недопустим неконтролируемый `1fr` — при длинном содержимом (заголовок, артикул, ссылка) контент раздувает контейнер.
- **Стандарт**:
  ```css
  grid-template-columns: minmax(0, 1fr) auto;
  /* или для адаптивных сеток карточек: */
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 280px), 1fr));
  ```

### 2.2. Обязательный `min-width: 0` для усечения текста
В любой ячейке сетки, где применяется `text-overflow: ellipsis`, контейнер имеет `min-width: 0;` для корректного расчета границ сжатия браузером:
```css
:where(.m3-label-truncated) {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

### 2.3. Канонические Grid-паттерны
- **Центрирование**:
  ```css
  :where(.grid-center) {
      display: grid;
      place-items: center;
  }
  ```
- **Разделение по краям (Split / Space-Between)**:
  ```css
  :where(.grid-split) {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      align-items: center;
      gap: var(--md-sys-spacing-3);
  }
  ```
- **Стек (Вертикальная колонка)**:
  ```css
  :where(.m3-stack) {
      display: grid;
      gap: var(--md-sys-spacing-3);
  }
  ```
- **Компактный горизонтальный ряд**:
  ```css
  :where(.grid-flow-col) {
      display: grid;
      grid-auto-flow: column;
      grid-auto-columns: max-content;
      align-items: center;
      gap: var(--md-sys-spacing-2);
  }
  ```

---

## 3. 📦 CSS Container Queries (`@container`) и M3 Window Size Classes

### 3.1. Micro Layout (Компоненты и Виджеты)
Адаптивность карточек, виджетов и диалоговых окон строится от ширины родителя через **Container Queries**:
```css
:where(.m3-card) {
    container-type: inline-size;
}

@container (max-width: 480px) {
    :where(.m3-card-body) {
        grid-template-columns: minmax(0, 1fr);
        gap: var(--md-sys-spacing-2);
    }
}
```

### 3.2. Macro Layout (Каркас страницы, Shell, Navigation)
Управляется `@media` по каноническим **M3 Window Size Classes**:
- **Compact** (`< 600px`): Мобильные телефоны в портретной ориентации. Нижняя навигация (Navigation Bar), 1 колонка.
- **Medium** (`600px – 839px`): Планшеты, складные устройства, малые POS. Боковая навигационная колонка (Navigation Rail).
- **Expanded** (`840px – 1199px`): Планшеты в альбомной ориентации, POS-терминалы, десктопы. Боковое меню (Navigation Drawer).
- **Large** (`1200px – 1599px`): Стандартные десктопы, широкоформатные киоски. Постоянный Drawer, двухколоночные рабочие зоны.
- **ExtraLarge** (`>= 1600px`): Большие мониторы, 4K POS, Digital Signage. Центрированный холст с максимальной шириной или мультипанельный интерфейс.

---

## 4. 🎨 Токены и темы Material Design 3

### 4.1. Использование токенов M3X
Все визуальные параметры привязаны к дизайн-токенам `Assets/wwwroot/css/tokens/`:
- **Цвета**: `var(--md-sys-color-surface)`, `var(--md-sys-color-primary)`, `var(--md-sys-color-on-surface)` и др.
- **Скругления**:
  - Кнопки: `var(--md-sys-shape-corner-full, 9999px)`
  - Карточки: `var(--md-sys-shape-corner-medium, 16px)` или `corner-large (24px)`
  - Диалоги и модальные окна: `var(--md-sys-shape-corner-extra-large, 28px)`
  - Фирменные экспрессивные шейпы: `var(--md-sys-shape-expressive-container, 24px 8px 24px 8px)`
- **Тени**: `var(--md-sys-elevation-level1)` ... `var(--md-sys-elevation-level5)`
- **Анимации**: `transition: transform var(--md-sys-motion-duration-short3) var(--md-sys-motion-easing-spring);`

### 4.2. Смена темы (Dark & Light Mode)
Поддерживается нативно через data-атрибут `data-theme` на корневом элементе:
```html
<html data-theme="dark"> <!-- По умолчанию в Socratic -->
<html data-theme="light">
```
Все переменные темы переключаются бесшовно без перезагрузки страницы.

---

## 5. 👆 Сенсорная эргономика (Touch-First)

Интерфейсы экосистемы Socratic работают на физических кассовых терминалах, киосках самообслуживания и мобильных устройствах.

1. **Минимальные зоны нажатия (Touch Targets)**:
   - Мобильный Web / Desktop: $\ge 48\times 48\text{px}$
   - POS-терминалы кассира: $\ge 48\times 48\text{px}$ (акцентные действия $\ge 56\text{px}$)
   - Киоски самообслуживания (Kiosk): $\ge 56\times 56\text{px}$
   - Кнопки быстрой оплаты (Fast Checkout / Pay): $\ge 64\text{px}$
2. **Отступы между активными элементами**:
   - Минимум $12\text{px}$ между центрами соседних интерактивных зон для предотвращения ошибочных нажатий.
3. **Исключение задержек сенсорного ввода**:
   - `touch-action: manipulation;` для мгновенного отклика без задержки двойного тапа (300ms delay).

---

## 6. 🏛️ Структура слоев `styles.css`

Корневой файл [`styles.css`](file:///d:/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/styles.css) подключает модули системы по 6 слоям каскада **Cascade Layers (`@layer`)**:

```css
/* Layer Precedence (Lower to Higher Priority: от абстрактного/базового к конкретному контексту) */
@layer tokens, foundations, components, widgets, pages, layouts;

/* 0. Typography Fonts */
@import "fonts/material-symbols-outlined.css";
@import "fonts/roboto.css";
@import "fonts/montserrat.css";

/* 1. Design Tokens (W3C DTCG Standard Types) */
@import "tokens/sizing.css" layer(tokens);
@import "tokens/spacing.css" layer(tokens);
@import "tokens/border.css" layer(tokens);
@import "tokens/outline.css" layer(tokens);
@import "tokens/positioning.css" layer(tokens);
@import "tokens/overflow.css" layer(tokens);
@import "tokens/typography.css" layer(tokens);
@import "tokens/shadow.css" layer(tokens);
@import "tokens/motion.css" layer(tokens);
@import "tokens/colors.css" layer(tokens);

/* 2. Foundations & Reset */
@import "foundations/base.css" layer(foundations);
@import "foundations/landmarks.css" layer(foundations);
@import "foundations/typography.css" layer(foundations);
@import "foundations/scrollbars.css" layer(foundations);
@import "foundations/animations.css" layer(foundations);

/* 3. Blazor Layouts (Page Skeletons & Shells) */
@import "layouts/app.css" layer(layouts);

/* 4. Page-Specific Styles (Macro Screen Contexts) */
@import "pages/about.css" layer(pages);
@import "pages/analytics.css" layer(pages);
@import "pages/auth.css" layer(pages);
@import "pages/chat.css" layer(pages);
@import "pages/detector.css" layer(pages);
@import "pages/docs.css" layer(pages);
@import "pages/help.css" layer(pages);
@import "pages/kiosk.css" layer(pages);
@import "pages/landing.css" layer(pages);
@import "pages/orders.css" layer(pages);
@import "pages/payment.css" layer(pages);
@import "pages/terminal.css" layer(pages);

/* 5. Smart Widgets & Composite Tools (Domain Organisms) */
@import "widgets/ads.css" layer(widgets);
@import "widgets/ai-blob.css" layer(widgets);
@import "widgets/cart.css" layer(widgets);
@import "widgets/chat.css" layer(widgets);
@import "widgets/commerce-modes.css" layer(widgets);
@import "widgets/detector.css" layer(widgets);
@import "widgets/faceid-enrollment.css" layer(widgets);
@import "widgets/faceid-viewfinder.css" layer(widgets);
@import "widgets/fiscal.css" layer(widgets);
@import "widgets/invoice.css" layer(widgets);
@import "widgets/kanban.css" layer(widgets);
@import "widgets/kiosk.css" layer(widgets);
@import "widgets/lang-selector.css" layer(widgets);
@import "widgets/map.css" layer(widgets);
@import "widgets/merchant-settings.css" layer(widgets);
@import "widgets/pricing-tiers.css" layer(widgets);
@import "widgets/qr-customizer.css" layer(widgets);
@import "widgets/seat-designer.css" layer(widgets);
@import "widgets/settings.css" layer(widgets);
@import "widgets/terminal.css" layer(widgets);

/* 6. Material 3 Component Primitives (Attribute-Driven UI Units) */
@import "components/avatar.css" layer(components);
@import "components/card.css" layer(components);
@import "components/color-picker.css" layer(components);
@import "components/file-picker.css" layer(components);
@import "components/markdown.css" layer(components);
@import "components/paginator.css" layer(components);
@import "components/picker-trigger.css" layer(components);
@import "components/skeleton.css" layer(components);
@import "components/smart-checkbox.css" layer(components);
@import "components/smart-dataview.css" layer(components);
@import "components/smart-filter.css" layer(components);
@import "components/smart-form.css" layer(components);
@import "components/smart-grid.css" layer(components);
@import "components/smart-toasts.css" layer(components);
```

---

## 7. 🔍 Инспекция качества рендеринга & Browser as Law

- **Рендеринг в браузере — единственный источник истины**:
  Корректность интерфейса подтверждается реальным состоянием DOM, вычисленными стилями (`computed styles`) и геометрией (`getBoundingClientRect`).
- **Отсутствие горизонтального скролла**:
  `scrollWidth <= clientWidth` во всех разрешениях матрицы устройств.
- **Матрица вьюпортов для проверки**:
  - Мобильные: `320x844` (критический минимум), `360x800`, `390x844`, `430x932`
  - Планшеты и POS: `768x1024`, `1024x768`
  - Десктопы: `1440x900`, `1920x1080`
- **Чеклист DoD для стилей**:
  1. `display: grid` для всех структурных компонентов.
  2. `minmax(0, 1fr)` для гибких колонок.
  3. `min-width: 0` для ячеек с усечением текста (`text-overflow: ellipsis`).
  4. Все цвета, отступы и радиусы получены из токенов `var(--md-sys-*)`.
  5. Минимальная интерактивная зона $\ge 48\text{px}$.
