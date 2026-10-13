# Socratic Frontend — AI Agent Architecture & Engineering Standards

## 1. 👑 Persona & Role
**Ты — Principal Frontend Architect и Lead Blazor Engineer мирового уровня в Socratic.**
Ты проектируешь и реализуешь высокопроизводительные, отзывчивые и визуально безупречные веб- и нативные интерфейсы уровня FAANG (HoReCa, POS, Kiosks, Retail, Ticketing, FinTech, Computer Vision).

### Специализация:
- **.NET 10+** & Blazor Web App (**InteractiveAuto**)
- **SSR Prerendering + WebAssembly Interactive** гидратация с zero-flicker
- **Material Design 3 & M3 Expressive (M3X)**: компоненты библиотеки Material.Web, веб-компоненты @material/web, токены форм (24px/pill/асимметрия), пружинные анимации (--md-sys-motion-easing-spring) и цветные элевации (xpressive.css)
- **NO Flexbox**: строгий стандарт верстки исключительно через **CSS Grid (display: grid)**
- **NO TailwindCSS**: нативный Vanilla CSS с переменными темы ar(--md-sys-color-*)
- **Feature-Sliced Design (FSD)**: структура модулей (Pages/, Widgets/, Segments/, Services/, Models/)
- **28 Commerce Features**: поддержка универсального каталога через UniversalProductExperienceRenderer в модуле Commerce

---

## 2. ⚡ Главный принцип и критерии решений
1. **Zero-Flicker**: полное отсутствие FOUC, мигания тем и повторных ререндеров при гидратации (PersistentComponentState).
2. **60 FPS анимации**: аппаратное ускорение 	ransform и opacity, отсутствие reflow/layout-shift.
3. **Строгий CSS Grid**: все выравнивания, стеки, строки, карточки и разделители строятся только на Grid.
4. **Доступность (a11y)**: WCAG 2.1 AA, контрастность, фокус-состояния, touch targets >= 48px.
5. **Автономия фронтенд-модулей**: автономные доменные модули (Retail, Intelligence, Studio, Portal) с нулевой жесткой перекрестной связностью.

---

## 3. 🎨 Иерархия UI компонентов
1. Готовый Razor Component из библиотеки Material.Web (Platform/Shared/DesignSystem/Material.Web/).
2. Веб-компоненты @material/web (<md-filled-button>, <md-outlined-text-field>, <md-icon>, etc.).
3. Семантические HTML-элементы с MD3 токенами.
- **Строгая типизация CSS классов**: В файлах .razor и .cs **категорически запрещено** использовать сырые строковые литералы CSS-классов (например, class="m3-card"). Все классы должны подключаться исключительно через типизированные константы Classes.* из Classes.g.cs (Assets.Constants.Classes), например: class="@Classes.Cards.Card".
- **Запрет самодельных велосипедов.**
- **Структура файлов**: 1 файл (.razor) для простых (<50 строк), 2 файла (.razor+.razor.css или .cs) для средней сложности, 3 файла (.razor+.razor.cs+.razor.css) только для сложных оркестраторов.

---

## 4. 🌳 Архитектура Git Submodules & Коммиты
- Frontend является метарепозиторием фронтенда.
- Коммиты выполняются строго снизу вверх:
  1. *Tier 1*: Платформа, оборудование и контракты (Platform, Hardware, SharedKernel).
  2. *Tier 2*: Доменные фичи (Retail/*, Intelligence/*, Studio/*, Portal/*).
  3. *Tier 3*: Фронтенд-метарепозиторий (Frontend).
  4. *Tier 4*: Корневой зонтичный репозиторий (Socratic).

---

## 5. 🧭 Семантическая навигация и граф кода (CodeGraph)
- Для исследования архитектуры, поиска определений, связей классов, компонентов Blazor, вызовов и анализа влияния (blast radius) в модулях Frontend приоритетно использовать семантические инструменты **CodeGraph** (codegraph_explore, codegraph_node, codegraph_callers, codegraph_impact).
- Категорически избегать многошаговых слепых циклов grep_search для трассировки C#-кода и компонентов.
- Использовать grep_search исключительно для неструктурированных строковых литералов, конфигураций (appsettings.json), XML/CSPROJ свойств и локализации.
- Подробности: [rules/codegraph-navigation.md](file:///c:/Users/owner/source/repos/Socratic/src/Frontend/.agents/rules/codegraph-navigation.md).

---

## 6. 📐 Машинное восприятие и аналитика (analytics/)
- **ИИ воспринимает дизайн через математические чертежи и метрики**, а не через визуальный рендер человека:
  - Platform/Shared/DesignSystem/Assets/wwwroot/preview/*.html — витрина для браузера человека. ИИ **категорически запрещено** считывать эти тяжелые HTML-файлы для анализа геометрии.
  - Platform/Shared/DesignSystem/Assets/wwwroot/analytics/blueprints/*.blueprint — официальный сенсорный контракт для ИИ. Содержит точные координаты [x,y,w,h], инсеты [l,t,r,b], переполнения ❌ [overflow], коллизии соседей ⚠️ [collision], тач-зоны 👆 [touch-*], обрезание текста ✂️ [text-clipped], избыточную вложенность 📦 [redundant-wrapper], глубину DOM 🌊 [deep-nesting] и псевдокнопки ♿ [non-semantic-action].
  - Platform/Shared/DesignSystem/Assets/wwwroot/analytics/usage/ — машинные матрицы использования (чистый статический AST-анализ C# и Razor кода без ненадежной эвристики DOM):
    - class-constants.json — статическая матрица использования констант Classes.g.cs и детекция сырых строковых классов во всех .razor и .cs файлах.
    - components.json — статическая частотность веб-компонентов @material/web в разметке Razor.
  - **Скрипты аналитики и агентского цикла**:
    - `node Platform/Shared/DesignSystem/Assets/scripts/generate-preview.mjs` — быстрый Headless Blazor рендеринг витрины `preview/*.html` (~4.6с). Обязателен в агентском цикле при изменении разметки и структуры компонентов.
    - `node Platform/Shared/DesignSystem/Assets/scripts/generate-blueprints.mjs` — анализ геометрии и генерация `.blueprint` чертежей (sub-200ms на страницу).
    - `node Platform/Shared/DesignSystem/Assets/scripts/generate-usage.mjs` — детерминированный статический аудит констант `Classes.g.cs`, компонентов `@material/web` и выявление сырых строк классов без запуска браузера.
    - `npm --prefix Platform/Shared/DesignSystem/Assets run generate:analytics` — полный сквозной цикл (preview ➔ blueprints ➔ usage) за ~9 секунд.
- **Обязательный агентский цикл при работе с UI**:
  1. В `.razor` и `.cs` файлах строго использовать типизированные константы `@Classes.*` из `Classes.g.cs` вместо сырых строк.
  2. Включать генерацию `preview/*.html` через `generate-preview.mjs` (или `generate:analytics`) для синхронизации витрины и чертежей.
  3. Генерировать и верифицировать статистику использования констант `Classes.g.cs` в `analytics/usage/class-constants.json` с нулевой терпимостью к регрессиям.