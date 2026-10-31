# Socratic Design System — Архитектура стилей (UI & Presentation)

## 1. 🏛️ Фундаментальные принципы архитектуры стилей Socratic

Вся визуальная презентация экосистемы Socratic централизованно сосредоточена в проекте **`Assets`** (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) и строится на **6 ключевых столпах**:

### 1.1. Централизованная нативная платформа (Native W3C Platform)
- **Единый центр стилизации**: все стили живут в `Assets/wwwroot/css/` с точкой входа [`styles.css`](file:///d:/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/styles.css).
- **Zero Build-Step для CSS**: чистый нативный W3C CSS без препроцессоров, утилитных генераторов и монолитных бандлов. Браузер напрямую парсит валидные спецификации W3C через параллельные мультиплексированные потоки HTTP/2 и HTTP/3.
- **Разделение ответственности**: разметка компонентов (`*.razor`) свободна от инлайн-стилей и тегов `<style>`. Стилизация детерминирована на уровне платформенных классов дизайн-системы.

### 1.2. 7-слойный детерминированный каскад W3C Cascade Layers (`@layer`)
Иерархия каскада зафиксирована декларацией `@layer` в начале `styles.css`:
```css
@layer foundations, tokens, components, widgets, layouts, pages, features;
```
- **Приоритет слоев**: нарастает слева направо. Правила из слоев `pages` и `features` естественным образом переопределяют стили `components`, `tokens` и `foundations`.
- **Предсказуемость каскада**: порядок импортов не создает конфликтов специфичности. Директива `!important` не требуется.

### 1.3. Онтология токенов W3C DTCG & Material Design 3 Expressive
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
- **Сквозная типизация в C#**: все CSS-классы, токены, иконки и ресурсы компилируются Roslyn генераторами в строго типизированные структуры (`Classes.g.cs`, `Icons.g.cs`, `Assets.g.cs`), обеспечивая 100% проверку компилятором без строковой магии.

### 1.4. Детерминированная геометрия: Pure CSS Grid & Container Queries
- **Двумерная математическая модель**: все сетки, колонки, ряды и стеки строятся исключительно на `display: grid`.
- **UI Geometry Contract**:
  - Каждая гибкая колонка защищена `minmax(0, 1fr)` (исключает переполнение контейнера).
  - Ячейки с усечением текста (`text-overflow: ellipsis`) снабжаются `min-width: 0;`.
- **CSS Container Queries (`@container`)**: Micro Layout (карточки, модальные окна, виджеты) адаптируется к ширине своего непосредственного родителя, а не экрана.
- **M3 Window Size Classes**: Macro Layout (каркас приложения, навигация, рабочие зоны) управляется адаптивной матрицей вьюпортов M3 (`Compact`, `Medium`, `Expanded`, `Large`, `ExtraLarge`).

### 1.5. Консолидация по селекторам и нулевая специфичность (`:where(...)`)
- Общие базовые примитивы группируются в мастер-селекторы `:where(...)` с весом `(0, 0, 0)`.
- Специфичные правила компонентов размещаются в их целевых файлах, сохраняя модульность и изолированность.

### 1.6. Сенсорная эргономика (Touch-First & High-Throughput POS/Kiosk)
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

Корневой файл [`styles.css`](file:///d:/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/styles.css) подключает модули системы по 7 архитектурным слоям:

```css
@layer foundations, tokens, components, widgets, layouts, pages, features;

/* 0. Шрифты */
@import "fonts/material-symbols-outlined.css";
@import "fonts/roboto.css";
@import "fonts/montserrat.css";

/* 1. Foundations & Reset */
@import "foundations/base.css" layer(foundations);
@import "foundations/typography.css" layer(foundations);
@import "foundations/scrollbars.css" layer(foundations);

/* 2. Design Tokens (W3C DTCG Standard Types) */
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

/* 3. Material 3 Component Primitives */
@import "components/buttons.css" layer(components);
@import "components/cards.css" layer(components);
@import "components/chips.css" layer(components);
@import "components/common.css" layer(components);
@import "components/dialogs.css" layer(components);
@import "components/expressive.css" layer(components);
@import "components/feedback.css" layer(components);
@import "components/forms.css" layer(components);
@import "components/invoice.css" layer(components);
@import "components/profile-dialog.css" layer(components);
@import "components/smart-data-view.css" layer(components);
@import "components/smart-enum.css" layer(components);
@import "components/smart-filter.css" layer(components);
@import "components/smart-form.css" layer(components);
@import "components/smart-toast.css" layer(components);
@import "components/tables.css" layer(components);

/* 4. Smart Widgets & Tools */
@import "widgets/ads.css" layer(widgets);
@import "widgets/booking-picker.css" layer(widgets);
@import "widgets/cart.css" layer(widgets);
@import "widgets/color-picker.css" layer(widgets);
@import "widgets/faceid-viewfinder.css" layer(widgets);
@import "widgets/lang-selector.css" layer(widgets);
@import "widgets/media-player.css" layer(widgets);
@import "widgets/qr-designer.css" layer(widgets);
@import "widgets/seat-designer.css" layer(widgets);

/* 5. Blazor Layouts */
@import "layouts/footer.css" layer(layouts);
@import "layouts/grid-layout.css" layer(layouts);
@import "layouts/header.css" layer(layouts);
@import "layouts/landing-layout.css" layer(layouts);
@import "layouts/map-layout.css" layer(layouts);

/* 6. Page-Specific Styles */
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

/* 7. Commercial Features */
@import "features/commerce-modes.css" layer(features);
@import "features/fiscal.css" layer(features);
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
