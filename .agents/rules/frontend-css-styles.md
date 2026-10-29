# Архитектура фронтенда: 3. Стили в *.css файлах

## 1. 🏛️ Золотой стандарт: Консолидация по селекторам в Assets

Вся стилизация пользовательского интерфейса экосистемы Socratic централизованно сосредоточена в проекте **`Assets`** (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) и компилируется через LightningCSS в единый оптимизированный бандл `material.bundle.min.css`.

Мы полностью **отказались от W3C CSS Modules в пользу консолидации по селекторам (Selector Consolidation)**: одинаковые CSS-декларации группируются в мастер-селекторы `:where(...)` с нулевой специфичностью (0, 0, 0).

> **ГЛАВНЫЙ ЗАКОН СТИЛЕЙ**:
> Стили живут исключительно в `*.css` файлах проекта `Assets`. Категорически запрещены компонентная CSS-изоляция (`.razor.css`), блоки `<style>` в разметке и инлайн-стили `style="..."`. Все лейауты строятся строго на **CSS Grid** (полный запрет Flexbox), токенах **Material Design 3** и **консолидации по селекторам**.

---

## 2. 🚫 Категорические запреты в стилизации

| Что запрещено | Почему запрещено | Как делать правильно |
|---|---|---|
| ❌ Локальные файлы `.razor.css` | Фрагментируют стили, создают runtime scope-атрибуты, ломают сжатие Brotli и единую дизайн-систему | Стили объявляются централизованно в `Assets/wwwroot/css/` через консолидацию селекторов (исключения: только `QuickGrid` и `Markdown`) |
| ❌ Блоки `<style>` в HTML/Razor | Загрязняют разметку, дублируют правила в DOM, не минифицируются сборщиком | Все правила выносятся в общую систему стилей `Assets` |
| ❌ Инлайн-стили `style="..."` | Высочайшая специфичность, ломают темы и адаптивность | Использовать CSS-классы дизайн-системы |
| ❌ Flexbox (`display: flex`) | Недетерминирован, баги сжатия (`flex-shrink`), blowout ячеек при длинном тексте | **Исключительно CSS Grid (`display: grid`)** |
| ❌ TailwindCSS | Засоряет DOM миллионом классов, конфликтует с токенами M3 | Vanilla CSS + консолидация по селекторам + токены M3 |
| ❌ Hardcoded цвета (`#1e293b`, `rgb(...)`) | Ломают светлую/темную тему и доступность | Только CSS-токены `var(--md-sys-color-*)` |
| ❌ Директива `!important` | Ломает каскад и предсказуемость специфичности | Стили группируются через `:where(...)` с весом (0,0,0) |

---

## 3. 📐 Геометрический контракт верстки: CSS Grid Only (Zero Flexbox)

Все структуры, выравнивания, стеки, ряды и карточки реализуются исключительно через **CSS Grid**.

### 1. Правило `minmax(0, 1fr)` (Защита от Blowout)
- **Запрещен голый `1fr`**: В любых гибких колонках или рядах голый `1fr` раздувается контентом (длинный заголовок, ссылка, SKU) и ломает верстку.
- **Обязательно**: `grid-template-columns: minmax(0, 1fr) auto;` или `repeat(auto-fill, minmax(min(100%, 280px), 1fr));`.

### 2. Обязательный `min-width: 0` для усечения текста
В любой ячейке сетки, где применяется `text-overflow: ellipsis`, контейнер **обязан** иметь `min-width: 0;`, иначе браузер не сможет рассчитать границы сжатия:
```css
:where(.m3-label-truncated) {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

### 3. Базовые Grid-примитивы (Замена Flexbox)
- **Центрирование**:
  ```css
  :where(.grid-center) {
      display: grid;
      place-items: center;
  }
  ```
- **Разделение лево / право (Split / Space-Between)**:
  ```css
  :where(.grid-split) {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      align-items: center;
      gap: 12px;
  }
  ```
- **Стек (Вертикальная колонка)**:
  ```css
  :where(.m3-stack) {
      display: grid;
      gap: 12px;
  }
  ```
- **Компактный горизонтальный ряд**:
  ```css
  :where(.grid-flow-col) {
      display: grid;
      grid-auto-flow: column;
      grid-auto-columns: max-content;
      align-items: center;
      gap: 8px;
  }
  ```

---

## 4. 📦 CSS Container Queries (`@container`)

Адаптивность виджетов, модальных окон и карточек строится **не от экрана браузера**, а от ширины родителя через **Container Queries**:

```css
:where(.m3-card) {
    container-type: inline-size;
}

@container (max-width: 480px) {
    :where(.m3-card-body) {
        grid-template-columns: 1fr;
        gap: 8px;
    }
}
### 4.1. Разграничение Macro Layout и Micro Layout
- **Macro Layout (Каркас страницы, Shell, Navigation)**: управляется `@media` по каноническим **M3 Window Size Classes**:
  - **Compact**: `< 600px` (смартфоны, мобильный POS) ➔ `@media (max-width: 599.98px)`
  - **Medium**: `600px – 839.98px` (планшеты портрет) ➔ `@media (min-width: 600px) and (max-width: 839.98px)`
  - **Expanded**: `840px – 1199.98px` (POS-терминалы 10"-12", планшеты ландшафт) ➔ `@media (min-width: 840px) and (max-width: 1199.98px)`
  - **Large**: `1200px – 1599.98px` (стандартный POS 15.6", киоски 1080p) ➔ `@media (min-width: 1200px) and (max-width: 1599.98px)`
  - **Extra-Large**: `$\ge 1600px$` (Back-office Desktop, 4K) ➔ `@media (min-width: 1600px)`
  > **Запрет ad-hoc брейкпоинтов**: Категорически запрещены случайные брейкпоинты (`768px`, `992px`, `1024px`, `1366px`).
- **Micro Layout (Компоненты, Карточки, Диалоги, Виджеты)**: управляется через **Container Queries** (`@container (max-width: ...)`).

---

## 5. 🪟 Модальные окна и оверлеи

Модальные окна (`MaterialDialog`, `BottomSheet`) строятся по трехслойной Grid-схеме:
```css
:where(.m3-dialog-surface) {
    display: grid;
    grid-template-rows: auto minmax(0, 1fr) auto;
    max-height: calc(100dvh - 48px);
}

:where(.m3-dialog-content) {
    overflow-y: auto;
    overscroll-behavior: contain;
}
```
> **Защита теней и меню**: Запрещено ставить слепой `overflow: hidden` на карточки с выпадающими меню и тенями `box-shadow`.

---

## 6. 🌈 Токены Material Design 3 Expressive (M3X) и темы

Все цвета, скругления, шрифты и анимации задаются через дизайн-токены Material 3 Expressive:

### Цветовая палитра:
- **Surface**: `var(--md-sys-color-surface)`, `var(--md-sys-color-surface-container)`, `var(--md-sys-color-surface-container-high)`
- **Primary**: `var(--md-sys-color-primary)`, `var(--md-sys-color-on-primary)`
- **Outline**: `var(--md-sys-color-outline)`, `var(--md-sys-color-outline-variant)`
- **Error**: `var(--md-sys-color-error)`, `var(--md-sys-color-on-error)`

### Темы (Light & Dark):
Тема переключается через атрибут `:root[data-theme="dark"]` и `:root[data-theme="light"]`. Контраст текста и иконок обязан соответствовать **WCAG 2.1 AA** (>= 4.5:1 для обычного текста, >= 3:1 для крупных элементов).

### Шкала выразительных скруглений (M3X Expressive Shapes):
- `--md-sys-shape-expressive-card: 24px;` — стандартные карточки
- `--md-sys-shape-expressive-button: 20px;` — интерактивные кнопки
- `--md-sys-shape-expressive-pill: 9999px;` — чипы, поисковые строки, pill-кнопки
- `--md-sys-shape-expressive-dock: 32px;` — плавающие панели действий
- `--md-sys-shape-expressive-dialog: 32px;` — модальные окна
- `--md-sys-shape-expressive-sheet: 32px 32px 0 0;` — нижние шторки (Bottom Sheets)

### Физика пружинных анимаций (Spring Motion Curves):
- `--md-sys-motion-easing-spring: cubic-bezier(0.175, 0.885, 0.32, 1.275);` — стандартный пружинный отклик
- `--md-sys-motion-easing-spring-snappy: cubic-bezier(0.2, 1.3, 0.4, 1.0);` — мгновенный упругий щелчок
- `--md-sys-motion-easing-spring-soft: cubic-bezier(0.3, 1.15, 0.5, 1.0);` — мягкая доводка списков

---

## 7. 👆 Эргономика и Touch Targets (POS, Kiosk, Mobile)

Интерфейсы Socratic работают на кассовых терминалах, киосках самообслуживания и мобильных терминалах:

| Сценарий / Оборудование | Минимальный размер Touch Target | Назначение |
|---|---|---|
| **Handheld / Mobile POS** | **48 × 48 px** | Чипы модификаторов, строки списков, иконки |
| **Kiosk Self-Service** | **56 × 56 px** | Вкладки категорий, карточки блюд, кнопки +/- |
| **Primary Pay / Checkout** | **64 px высота** | Главная кнопка «Оплатить», «Завершить заказ» |
| **Touch Spacing (Зазор)** | **>= 12 px** | Безопасное расстояние между соседними кнопками |

- **Тактильный отклик**: На `:active` применяется мгновенное сжатие `scale(0.96)` с анимацией `50ms` для подтверждения нажатия.
- **Scroll Containment**: Для каталогов и чеков обязательны `overscroll-behavior: contain;` и `-webkit-overflow-scrolling: touch;`.

---

## 8. 🏛️ Консолидация по селекторам & Философия единственной декларации

Мы полностью **отказались от W3C CSS Modules в пользу консолидации по селекторам (Selector Consolidation)**:

1. **Мастер-селекторы `:where(...)`**:
   Все классы, разделяющие одно стилевое поведение или свойство, объединяются в общие мастер-селекторы через `:where(...)`:
   ```css
   :where(.m3-card, .checkout-panel, .ticket-widget) {
       display: grid;
   }
   ```
2. **Нулевая специфичность `(0, 0, 0)`**:
   Псевдокласс `:where(...)` гарантирует вес специфичности `(0, 0, 0)`. Это позволяет легко кастомизировать и переопределять свойства без «войны специфичностей» и без использования `!important`.
3. **Метод Selector Append (Добавление селектора)**:
   При добавлении нового компонента или страницы не создается отдельный разрозненный CSS-блок с дублирующими свойствами. Новый селектор просто дописывается через запятую в уже существующий мастер-селектор `:where(...)`.
4. **Single-Declaration (Единственная декларация)**:
   Каждая CSS-декларация (свойство + значение) встречается ровно один раз во всей библиотеке. Это исключает дублирование, минимизирует CSS-бандл и обеспечивает максимальный коэффициент сжатия Brotli (до 85–90%).
5. **7-слойная структура в `Assets/wwwroot/css/styles.css`**:
   - `foundations/` — сброс, базовые стили, типографика, скроллбары;
   - `tokens/` — токены цветов, скруглений, теней, анимаций, светлая и темная темы;
   - `components/` — канонические примитивы Material 3;
   - `widgets/` — специализированные виджеты (booking-picker, cart, qr-designer, etc.);
   - `layouts/` — макеты страниц (header, footer, grid-layout, etc.);
   - `pages/` — стили страниц экранов;
   - `features/` — стили сценариев коммерческих модулей (commerce-modes, fiscal).
6. **Flattening & Псевдоэлементы**:
   Запрещены вложенные `:where(:where(...))`. По стандарту W3C Selectors Level 4 псевдоэлементы (`::before`, `::after`) объявляются снаружи `:where(...)`.
7. **Строгий запрет `!important`**:
   Полный запрет `!important` во всех CSS-правилах (исключения строго 3: WCAG `prefers-reduced-motion`, HTML5 `[hidden]` / диалоги `:not([open])`, и изоляция иконок).

---

## 9. 🔍 Инспекция качества рендеринга & Browser as Law

- **Рендеринг в браузере — единственный источник истины**:
  Корректность интерфейса проверяется не по тексту разметки, а по реальному состоянию DOM, computed styles и bounding rects.
- **Отсутствие горизонтального скролла**:
  `scrollWidth <= clientWidth` во всех разрешениях адаптивной матрицы.
- **Матрица вьюпортов для проверки**:
  - Мобильные: `320x844` (критический минимум), `360x800`, `390x844`, `430x932`
  - Планшеты и POS: `768x1024`, `1024x768`
  - Десктопы: `1440x900`, `1920x1080`
- **Чеклист DoD для стилей**:
  1. `display: grid` для всех структур (Zero Flexbox).
  2. `minmax(0, 1fr)` для гибких колонок (Zero голый `1fr`).
  3. `min-width: 0` для ячеек с усечением текста (`text-overflow: ellipsis`).
  4. Touch targets >= 48x48px (>= 56x56px для киосков).
  5. Цвета только через M3-токены `var(--md-sys-color-*)` (Zero HEX/RGB).
  6. Контраст WCAG 2.1 AA в обеих темах (Light и Dark).
  7. Консолидация через `:where(...)` в `Assets/wwwroot/css/` (Zero `.razor.css`, Zero `<style>`, Zero `style="..."`).
  8. Успешный запуск `npm run refresh` в `Assets` без новых нарушений.
