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

1. **Готовый Razor Component из библиотеки `Material.Web`** (`src/Shared/Material.Web/`):
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
- **Дизайн-токены (Vanilla CSS, NO Tailwind)**:
  - Запрещен TailwindCSS. Только изолированный Vanilla CSS в `.razor.css`.
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

### Паттерны замены Flexbox на Grid:
- **Вертикальный стек (колонка)**:
  ```css
  /* ВМЕСТО: display: flex; flex-direction: column; gap: 12px; */
  display: grid;
  gap: 12px;
  ```
- **Горизонтальный ряд (строка)**:
  ```css
  /* ВМЕСТО: display: flex; flex-direction: row; gap: 8px; */
  display: grid;
  grid-auto-flow: column;
  gap: 8px;
  align-items: center;
  ```
- **Разделение лево / право (Space-Between)**:
  ```css
  /* ВМЕСТО: display: flex; justify-content: space-between; */
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
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
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 16px;
  ```

---

## 7. 📁 Архитектура файлов компонентов (1, 2 или 3 файла)
> **ПРАГМАТИЧНАЯ ДЕКОМПОЗИЦИЯ**: Запрещено догматично разделять абсолютно каждый тривиальный компонент на 3 файла (`.razor`, `.razor.cs`, `.razor.css`). Это раздувает проект (file sprawl), ухудшает навигацию и снижает скорость разработки без какой-либо архитектурной пользы.

Количество файлов выбирается строго по сложности компонента:

### 1 Файл (`.razor` с блоком `@code`)
- **Когда использовать**:
  - Атомарные, презентационные («dumb») и UI-компоненты низкого уровня.
  - Разметка до ~50 строк, от 1 до 3 параметров, минимальная inline-логика (эмит событий, переключение флага).
  - Стили либо отсутствуют, либо покрываются глобальными Grid-примитивами (`.grid-center`, `.grid-split`) и токенами Material Design 3.
- **Примеры**: бейджи статусов, простые кнопки-обертки, иконки с подписью, слоты для карточек.

### 2 Файла (`.razor` + `.razor.css` ИЛИ `.razor` + `.razor.cs`)
- **Когда использовать**:
  - **`.razor` + `.razor.css`**: Компоненты средней сложности с уникальной локальной версткой (15–40 строк изолированного CSS), но простой C#-логикой (до 30 строк), которая комфортно читается прямо в блоке `@code`.
  - **`.razor` + `.razor.cs`**: Логически насыщенные компоненты, не требующие собственного изолированного CSS (вся верстка покрыта готовыми компонентами `Material.Web` и CSS Grid утилитами).
- **Примеры**: карточки сущностей, диалоговые окна с базовыми полями ввода, тулбары.

### 3 Файла (`.razor` + `.razor.cs` + `.razor.css`)
- **Когда использовать**:
  - Крупные бизнес-фичи, страницы, смарт-компоненты и оркестраторы (Feature Slices).
  - Наличие сложного жизненного цикла (`OnInitializedAsync`, `PersistentComponentState`, `IAsyncDisposable`).
  - Множество DI-инъекций, подписки на события/шину, валидация форм, JS-интероп, сложная математика координат/графиков.
  - Объемный C#-код (> 50 строк) и объемный изолированный CSS (> 40 строк с кастомными анимациями/лейаутом).
- **Примеры**: `MapPage`, `SmartSchedulePicker`, `OrderGrid`, `PosTerminalWorkspace`.

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


