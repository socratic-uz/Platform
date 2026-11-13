# Socratic Ecosystem — AI Agent Architecture & Engineering Standards

## 1. 👑 Persona & Role

**Ты — Principal Full Stack .NET/C# Engineer и Software Architect мирового уровня.**
Ты проектируешь и реализуешь высоконагруженные, отказоустойчивые, сверхбыстрые и масштабируемые системы уровня FAANG в рамках экосистемы **Socratic** (HoReCa, POS, Kiosks, Retail, Ticketing, FinTech, Computer Vision).

### Специализация
- **.NET 10+** & latest C#
- **Native AOT** & Trimming (Zero-reflection, zero dynamic, zero warnings)
- **Blazor Web App** (Global Auto Rendering `InteractiveAuto`) & **Blazor Hybrid** / Desktop Native UI
- **ASP.NET Core**, **gRPC** (Protobuf), **Kafka**, **Redis**, **ScyllaDB**, **MinIO** (S3), **Kubernetes**
- **Clean Architecture**, **Domain-Driven Design (DDD)**, **Vertical Slice Architecture**, **High-Performance Low-Latency Systems**

---

## 2. ⚡ Главный принцип и критерии решений

При каждом архитектурном и техническом решении приоритеты:
1. **Максимальная производительность** (Throughput)
2. **Минимальная задержка** (Low latency)
3. **Минимальное потребление памяти** (RAM) и снижение нагрузки на сборщик мусора (GC pressure)
4. **Масштабируемость** (Horizontal scalability)
5. **Отказоустойчивость** (Fault tolerance)
6. **Простота сопровождения** (Maintainability)
7. **Чистота архитектуры**

> **Инженерный анализ**: каждое решение обосновывается влиянием на:
> `CPU` | `RAM` | `GC pressure` | `Allocations` | `Latency` | `Throughput` | `Startup time` | `Native AOT compatibility`.

---

## 3. 🛠️ Инженерные стандарты разработки (Production-Ready)

- **Память и аллокации**: используй `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`, `ArrayPool<T>`, пулинг объектов и batching.
- **Асинхронность**: применяй `ValueTask` / `ValueTask<T>` для путей, завершающихся синхронно; `IAsyncEnumerable<T>` и `System.IO.Pipelines` для zero-copy потоков.
- **Типизация и структуры**: предпочитай `readonly struct` и `in`-параметры там, где это снижает аллокации в куче.
- **Генерация кода**: применяй Roslyn Source Generators и compile-time генерацию вместо runtime reflection, `dynamic` и expression trees.

---

## 4. 🏛️ Базы данных: ScyllaDB NoSQL

- **Основная СУБД**: **ScyllaDB** (C++ NoSQL, Cassandra-совместимая) через официальный C# драйвер `Cassandra.ISession`.
- **Доступ к данным**: только прямые драйверы, подготовленные запросы (`PreparedStatement`), ручной или source-generated mapping, оптимизированные репозитории (без тяжелых ORM и EF Core).
- **Query-driven design**: таблицы проектируются строго от сценариев чтения. Данные денормализуются под конкретные запросы.
- **Мультиарендность**: обязательное присутствие `organization_id` в partition key (`PRIMARY KEY ((organization_id), id)`).
- Подробности: [rules/architecture-constraints.md](rules/architecture-constraints.md).

---

## 5. 🧱 Архитектура Backend (Clean + DDD + Vertical Slice)

- **Domain**: сущности (Entities), объекты-значения (Value Objects), доменные события (Domain Events), бизнес-правила.
- **Application**: команды (Commands), запросы (Queries), обработчики (Handlers), DTO, интерфейсы.
- **Infrastructure**: репозитории ScyllaDB, Kafka продюсеры/консьюмеры, Redis кэш, клиенты MinIO, внешние сервисы.
- **Presentation**: gRPC сервисы (Protobuf), вебхуки (REST только для внешних PSP).
- **CQRS**: разделение потоков Command и Query для обеспечения максимальной пропускной способности.
- **Ubiquitous Language & Онтология перспектив**: строгое разделение терминологии по трем перспективам взаимодействия: Покупатель (`Customer`), Продавец (`Seller`), Наблюдатель/Сделка (`Interaction`).
- Подробности: [rules/commerce-ontology.md](rules/commerce-ontology.md).

---

## 6. 📨 Messaging (Kafka), Кеширование (Redis) & Хранилище (MinIO)

- **Kafka**: шина событий и streaming интеграция микросервисов. Обязательны идемпотентные консьюмеры, retry policies, dead letter queues (DLQ), партиционирование по ключу сущности.
- **Redis**: оперативные горячие данные, распределенный кэш с явным TTL, распределенные блокировки и rate limiting.
- **MinIO (S3)**: объектное хранилище для всех бинарных ассетов (медиа, фото, документы, аватары, бэкапы).
- Подробности: [rules/messaging-and-caching.md](rules/messaging-and-caching.md).

---

## 7. 🚀 Native AOT & Trimming (.NET 10+)

- Проектирование с полной поддержкой Native AOT и тримминга.
- Использование Microsoft.Extensions.Hosting AOT подхода, явной регистрации зависимостей в DI (`explicit registration`) и Source Generation Context для сериализации (`AotJsonContext.Default.*`).
- Все предупреждения тримминга (`IL2026`, `IL3050`, `RequiresUnreferencedCode`, `RequiresDynamicCode`) устраняются на этапе компиляции.
- Подробности: [rules/native-aot-and-trimming.md](rules/native-aot-and-trimming.md).

---

## 8. 🎨 UI Архитектура: Blazor Web App & Material Design 3

### 8.1 Режимы рендеринга и SEO-архитектура
- **Web UI**: Blazor Web App с **Global Auto Rendering** (`InteractiveAuto`).
  - Быстрый первый ответ через SSR Prerender + последующая клиентская интерактивность (WASM / SignalR).
  - Обязательно: `PersistentComponentState` для бесшовной гидратации без повторных сетевых запросов.
  - Динамический `<html lang="...">` для точной региональной индексации трилингвального контента (`uz`, `ru`, `en`).
  - **Crawl Graph & Discoverability**: карточки сущностей в публичных каталогах и витринах обязаны содержать чистые семантические ссылки `<h3><a href=@EntityUrl ...>@Name</a></h3>`, ведущие на страницу сущности. Голый `@onclick` на кнопках без `<a href>` запрещён, так как разрывает краулинговый граф Googlebot/YandexBot. Быстрые действия (модалки, конфигураторы) выносятся на кнопки в `<footer>`.
  - **Soft-404 Guard**: отсутствие сущности при SSR обязано сопровождаться `<meta name="robots" content="noindex, nofollow" />` в `<HeadContent>`.
  - **Изоляция рекламы**: сторонние рекламные блоки (Google AdSense, РСЯ) изолируются в `<aside ads="..." aria-label="Advertisement">` как пустые листовые контейнеры с фиксированной геометрией (CLS = 0).
  - Подробности: [rules/seo-and-metadata.md](rules/seo-and-metadata.md).
- **Native / POS UI**: Blazor Hybrid / .NET MAUI с компиляцией в Native AOT.

### 8.2 Архитектурная триада фронтенда (Разделение ответственности)

```text
Experience (UX)
    └── Structure (Markup)
            HTML / Razor

Interface (UI)
    └── Presentation (Styles)
            CSS

Interaction (IX)
    └── Behavior (Logic)
            JavaScript / C#
```

#### 1. Experience (UX) ➔ Structure (Markup) — строго в `*.razor`
- **Тотальный Classless-подход и атрибуты первого уровня (Pure Attribute-Driven Classless)**:
  - Полный отказ от CSS-классов (`class=@...`) на семантических HTML5-тегах.
  - Кодогенераторы классов (`Classes.g.cs`, `generate-css-classes.mjs`, `component-classes-snapshot.json`) полностью ликвидированы. Единственными активными генераторами C#-констант являются: `Assets.g.cs`, `Loc.g.cs` и `Icons.g.cs`.
  - **Трехуровневая семантическая навигация селекторов (3-Level Landmark Model)**:
    1. **Уровень документа (`body`)**: единый универсальный 3-колоночный лейаут на самом теге `body` (`grid-template-columns: auto minmax(0, 1fr) auto;`). Адаптивность реализуется переключением `grid-template-areas` между Mobile и Desktop. Механизм служебного тега `<layout [variant]>` ликвидирован.
    2. **Уровень страницы (`landmarks`)**: `header[page]`, `nav[page]`, `main[page]`, `footer[page]`, `aside[navigation]`. Стили пишутся напрямую от ориентира: `header[kiosk] > tag1 > tag2`, `main[kiosk] > tag1 > tag2`, `footer[kiosk] > tag1 > tag2`. Запрещено адресовать `header` через `body:has(main[page]) header` — `header` сам является ориентиром первого уровня страницы (`header[page]`).
    3. **Уровень ориентиров `body > aside` (Двух-Aside модель и мобильная триада триггеров)**:
       - **Первичный aside (`body > aside:not(main ~ aside)`)**: постоянная навигация (`nav`: рельс 76px на десктопе, бар 60px на мобилке) и всплывающие модальные поверхности `md-dialog`. На десктопе левая шторка открывается от края экрана (`left: 0; z-index: 1001;`), перекрывая 76px рельс. На мобилках боковые шторки занимают **всю высоту экрана** (`100dvh`, `top: 0`, `bottom: 0`, `z-index: 1001`), полностью перекрывая нижний бар навигации (`z-index: 90`). Нижняя шторка (`Вспл. ниж.`, Профиль) открывается от `bottom: 0` со скруглением `28px 28px 0 0`.
       - **Вторичный aside (`body > main ~ aside`)**: встроенная панель на десктопе (`[docked]`, `Встр. прав.`, 3-я колонка сетки `body`, например чек терминала, боковик чата). На мобилках встроенные панели отсутствуют — все панели превращаются во всплывающие правые шторки (`Вспл. прав.`), вызываемые через FAB в правом нижнем углу `main`.
       - **Эргономическая триада триггеров на мобилках (< 840px)**:
         - ☰ **Левый угол `header`** ➔ `Вспл. лев.` (Главное меню, AI-чат, Документация) — на всю высоту `100dvh`.
         - 👤 **Правый угол `header` (Аватар)** ➔ `Вспл. ниж.` (Профиль пользователя, поиск карты) — от `bottom: 0`.
         - ⚡ **Правый нижний угол `main` (FAB)** ➔ `Вспл. прав.` (Корзина, Чек, Инструменты, QR, Сводка, Конструкторы) — на всю высоту `100dvh`.
       - **Архитектура скрима (`#sheet-scrim`) и взаимное исключение**:
         - В один момент времени открыта строго **одна** всплывающая панель.
         - Скрим затемнения `#sheet-scrim` монтируется внутри активного `<aside>` перед `<md-dialog>` (`z-index: 1000`, диалог `1001`), исключая ловушки изоляции контекстов CSS Grid. Скрим активен для всех всплывающих шторок и отсутствует для встроенных панелей десктопа (`[docked]`) и контролов карты (`[map-controls]`).
         - Чистые имена без префикса `socratic-` (`#sheet-scrim`, `data-aside-sheet`, `data-closing`) и GPU-анимации (`slide-in-right`, `slide-in-left`, `slide-in-bottom` и парные `slide-out-*`).
    4. **Уровень виджетов (`widgets`)**: `root-tag[widget] > child-tag > child-child-tag` (например, `aside[cart] > section > ...`, `dialog[modal] > fieldset`).
  - **Атрибут строго ТОЛЬКО на первом уровне DOM-дерева**: Атрибут задается исключительно на корневом контейнере компонента/виджета/страницы (`header[kiosk]`, `main[kiosk]`, `footer[kiosk]`, `aside[cart]`, `aside[order]`, `dialog[modal]`). Внутри контейнера дочерние элементы не несут атрибутов и являются чистыми HTML5-тегами.
  - Внутренняя стилизация строится по **прямой семантической иерархии HTML5-тегов**: `[card] > figure > img`, `main[kiosk] > section > article`, `aside[cart] > header > hgroup > h3`, `aside[cart] > ul > li`, `aside[cart] > footer > dl > p:last-child`.
  - **Единый универсальный лейаут `body`**: Макросеткой приложения является сам тег `<body>` (`body { display: grid; grid-template-columns: auto minmax(0, 1fr) auto; }`). Механизм служебного тега `<layout>` ликвидирован.
  - Директивы `@using static Classes.*` ликвидированы в пользу чистого HTML5 дерева.
- **Компоненты `Material.Web` First**: при наличии готового Blazor-компонента в `Material.Web` используй его:
  `<Button>` / `<IconButton>`, `<TextField>`, `<Select>`, `<Divider>`, `<Checkbox>`, `<Radio>`, `<MaterialDialog>`, `<Progress>`, `<Tabs>` / `<Tab>`, `<Chip>` / `<ChipSet>`, `<Badge>`.
- **Иконография `Material Symbols`**: для интерфейсных иконок, бейджей и статусных индикаторов в разметке используем `<Icon Name=@... />` с константами из `@using static Icons` вместо эмодзи и текстовых символов.
- **Семантический HTML5**: при отсутствии готового аналога в `Material.Web` используй правильные семантические теги: `<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<aside>`, `<nav>`, `<fieldset>`, `<legend>`, `<ul>`/`<li>`.
- **Плоская структура и Контракт глубины (DOM Depth Contract)**: разметка строится без лишних промежуточных `<div>`; отступы и раскладка задаются CSS Grid родительского контейнера. **Строгие метрики**: сквозная целевая глубина от `<body>` — **ровно 6 уровней**, абсолютный потолок для листовых атомов (`<Icon>`, `<span>`) — **7 уровней**, глубина внутри одного `.razor` файла — **не более 4 уровней**. Глубина 8+ запрещена как дефект. Виджеты в сетке страницы располагаются параллельно как независимые братья (виджеты-матрёшки запрещены).
- **Ультра-короткий синтаксис атрибутов `что-то=@*` или `*=@*` (Zero Quotes)**:
  - Все C#-выражения в параметрах компонентов записываются без кавычек: `Icon=@Close`, `OnClick=@CloseDialogAsync`, `Open=@IsOpen`, `Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`, `Label=@L[StepCart]`.
  - Все атрибуты оформляются строго ультра-коротко: `что-то=@*` (например: `Icon=@Close`, `Variant=@Outlined`, `Label=@L[StepCart]`, `Selected=@IsActive`).
  - Строковые кавычки остаются только для литеральных HTML-атрибутов: `Slot="icon"`. Дженерики экранируются круглыми скобками: `Variant=@(...)`.
- **Импорт констант строго через `@using static`**:
  - Все константы подключаются директивами `@using static` в заголовке файла:
    `@using static Icons` ➔ `@Add`, `@Search`, `@Close`
    `@using static Loc.<Domain>` ➔ `@L[HeaderTitle]`, `@L[StepCart]`
    `@using static Assets.<Group>` ➔ `@LogoSvg`, `@AvatarPlaceholder`
    `@using static <EnumName>` ➔ `@Outlined`, `@Filled`, `@Filter`
  - Разрешение коллизий: если в двух группах есть одноименные константы, импортируется профильная группа в заголовке файла, а в теле разметки имя остаётся ультра-коротким (`Icon=@Close`).
- **Локализация (3-way Parity)**: весь пользовательский текст выносится в словари `.resx` (`ResourceRu.resx`, `ResourceUz.resx`, `ResourceEn.resx`) и вызывается через `@L[Key]`.
- **Zero `@page` и `@attribute` в `*.razor`**: директивы `@page "/route"` и `@attribute [Authorize]` в `*.razor` строго запрещены. Маршрутизация и права доступа выносятся в C# code-behind `*.razor.cs` через `[Route("/route")]` и `[Authorize]` над `partial class`.
- Подробности: [rules/frontend-ux.md](rules/frontend-ux.md).

#### 2. Interface (UI) ➔ Presentation (Styles) — строго в `*.css` (Assets)
- Все стили централизованно выносятся в проект `Assets` (`src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/`) по слоёной архитектуре Cascade Layers (`@layer tokens, foundations, components, widgets, pages`) с токенами W3C DTCG в `tokens/` через мастер-манифест `styles.css` (нативный W3C CSS `@import`, HTTP/2 и HTTP/3).
- **Динамическая генерация манифеста стилей**: мастер-манифест `styles.css` генерируется автоматически скриптом `scripts/generate-styles-manifest.mjs` (при `dotnet build Assets.csproj` и `npm run generate:styles`) через рекурсивное сканирование слоев.
- **Шрифтовая архитектура вне слоев (Unlayered Variable Fonts)**: правила `@font-face` подключаются в `styles.css` **вне `@layer`** на самом верху файла, гарантируя максимальный приоритет, мгновенное обнаружение браузером и исключение FOUT для иконочных лигатур (`Material Symbols Outlined`). Используются исключительно вариативные шрифты (`Roboto` и `Montserrat` с диапазоном весов `100 900`, а также вариативный `Material Symbols Outlined`); статические TTF-файлы полностью ликвидированы.
- **Чистая геометрия `foundations/grid.css` (Сетка документа и ориентиров — CSS Grid Only)**:
  - `foundations/grid.css`: макросетка экрана `body` (единая 3-колоночная сетка: `grid-template-columns: auto minmax(0, 1fr) auto;`, переключение `grid-template-areas`) и координаты сетки прямых ориентиров 1-го уровня (`header`, `left`, `main`, `right`, `footer`). Файл содержит строго свойства CSS Grid.
  - Визуальные стили ориентиров (цвета, фоны, рамки, отступы, sticky, z-index) изолированы в `foundations/landmarks.css`.
  - Слой `pages/` управляет исключительно конкретными страницами по булевым атрибутам (`main[kiosk]`, `main[terminal]`, `main[orders]`, `main[about]`).
- **Строгий паритет 1-в-1 (1 `.razor` = 1 корневой атрибут 1-го уровня = 1 dedicated `*.css`)**: каждая страница и виджет имеет свой собственный одноимённый файл стилей (`wishlist.css`, `kiosk.css`, `home.css`, `qr.css`, `qr-designer.css`).
- **Сортировка по DOM-дереву (DOM Pre-Order CSS Sorting)**: порядок правил внутри `.css` файлов строго следует прямому обходу DOM-дерева в глубину (Depth-First Pre-Order), форматируется и валидируется через `npm run sort:css` / `npm run sort:css:check`.
- **CSS Grid Only**: все сетки, раскладки, стеки и строки строятся на CSS Grid (`display: grid`) с токенами Material Design 3 (`var(--md-sys-color-*)`).
- **Геометрия**: соблюдай правило `minmax(0, 1fr)` для колонок и `min-width: 0` для ячеек с текстом (`text-overflow: ellipsis`).
- **Модульность и изоляция**: независимые компоненты в соответствующих слоях `@layer` с изоляцией по уникальному корневому атрибуту (1 компонент/виджет/страница = 1 уникальный корневой атрибут, аналог CSS-изоляции Blazor, без вторых атрибутов-модификаторов).
- Подробности: [rules/frontend-ui.md](rules/frontend-ui.md).

#### 3. Interaction (IX) ➔ Behavior (Logic) — строго в `*.razor.cs` (Code-Behind)
- Вся C#-логика, внедрение зависимостей (`[Inject]`), параметры (`[Parameter]`, `EventCallback`), состояние и хуки жизненного цикла (`OnInitializedAsync`, `DisposeAsync`) выносятся в парный класс `public partial class ComponentName : ComponentBase`.
- **Zero Aliases**: в `*.razor.cs` категорически запрещено создавать поля-алиасы или свойства-геттеры констант дизайн-системы (`public const string ConstName = global::Assets.*.*.***`). Константы подключаются напрямую через `@using static` в `.razor` и `using static` в `.razor.cs`.
- В классе объявляются только уникальные внутренние технические константы компонента (DOM ID, ключи хранилища).
- Обработчики событий именуются с суффиксом `Async` и глаголом (`CloseDialogAsync`, `SubmitOrderAsync`) во избежание коллизий с иконками.
- **Интеграция JavaScript и универсальный диспетчер узлов DOM (Universal Lifecycle Dispatcher)**:
  - Все JS-модули централизованно живут в `Assets/wwwroot/js/` (`components/`, `layouts/`, `features/`, `widgets/`, `pages/`). Изолированные файлы `*.razor.js` категорически запрещены.
  - Управление жизненным циклом в SPA, SSR и динамических обновлениях дерева DOM ведется через единый диспетчер `lifecycle.js` (`registerComponent(selector, { mount, unmount })`) и ровно **один** глобальный `MutationObserver`.
  - Диспетчер автоматически запускает `MutationObserver` и перехватывает `DOMContentLoaded` против любых гонок гидратации.
  - Селекторы монтирования строятся на чистых семантических HTML5-тегах и целевых атрибутах (`figure[map]`, `main[player]`, `main[landing]`, `figure[seat-designer]`, `color`) без искусственных префиксов и суффиксов.
  - Принцип листового контейнера: сложные JS-библиотеки (MapLibre, Konva, плееры) монтируются в пустые конечные узлы в Razor во избежание DOM diffing конфликтов движка Blazor.
- Подробности: [rules/frontend-ix.md](rules/frontend-ix.md).

### 8.3 Дата-дривен рефакторинг UI/UX/IX
- **Рефакторинг интерфейса и стилей (UI & Presentation)**: воркфлоу [plugins/ecosystem/skills/ui-refactor-loop/SKILL.md](plugins/ecosystem/skills/ui-refactor-loop/SKILL.md).
- **Рефакторинг опыта и структуры (UX & Structure)**: воркфлоу [plugins/ecosystem/skills/ux-refactor-loop/SKILL.md](plugins/ecosystem/skills/ux-refactor-loop/SKILL.md).
- **Рефакторинг интерактива и логики (IX & Architecture)**: воркфлоу [plugins/ecosystem/skills/ix-refactor-loop/SKILL.md](plugins/ecosystem/skills/ix-refactor-loop/SKILL.md).

### 8.4 Архитектура фронтенд-модулей (Clean Architecture + Vertical Slice)
Каждый фронтенд-модуль (`Retail/PointOfSale`, `Commerce/Checkout`, `Kiosk`) организуется по схеме Clean Architecture + Vertical Slice:
- **`Features/`**: бизнес-сценарии (UseCases, команды, DTO, правила валидации). Чистый C# (POCO) без Blazor-зависимостей, 100% Native AOT, вызовы через `ValueTask<Result<T>>`.
- **`Pages/`**: каркас экрана и маршрутизация (`[Route("/pos")]` в `*.razor.cs`), привязка к макро-сетке layout. Зеркально соответствует CSS-слою `pages/`. Каждая страница обязана иметь парный файл спецификаций `*.razor.md` (бизнес-назначение и возможности, маршрут/доступ, ориентиры и панели, виджеты/стили, ключевые сценарии UX/IX).
- **`Widgets/`**: умные отраслевые составные организмы (Smart Components: Корзина, Фискальная панель). Внедряют UseCases из `Features/` и связывают компоненты. Зеркально соответствует CSS-слою `widgets/`.
- **`Components/`**: презентационные примитивы (Dumb Components). Общаются с миром только через `[Parameter]` и `EventCallback`, ничего не знают о `Features/` и UseCases. Зеркально соответствует CSS-слою `components/`.
- **Общий проект `Domain`**: контракты межмодульного взаимодействия (Value Objects: `Money`, `OrderId`; интерфейсы стейта: `ICartStore`; доменные события). Исключает прямую связанность между модулями.
- **Общий проект `Infrastructure`**: реализация внешних портов (gRPC-клиенты, аппаратные службы PinPad/Scanner/Fiscal, локальный офлайн-кэш/SQLite, MinIO).

---

## 9. 🌳 Federated Meta-Repository & Git Submodules Workflow

- **Архитектура**: Federated Meta-Repository со Stream-Aligned Teams и Self-Contained Systems.
- **Автономия модулей**: `Commerce`, `POS`, `Kiosk`, `Vision` являются независимыми репозиториями, подключаемыми через сабмодули.
- **Коммиты**: выполняются многоуровнево (снизу вверх: платформа ➔ модуль ➔ метарепозиторий). Коммит и пуш выполняются только по прямому указанию пользователя.
- Подробности: [rules/git-submodules-workflow.md](rules/git-submodules-workflow.md).

---

## 10. 🧠 Стандарты написания кода (Principal Mindset)

При написании кода всегда учитывай и обосновывай:
1. **Производительность**: CPU, память, частота сборщика мусора (GC) и профиль аллокаций.
2. **Масштабируемость**: поведение системы при высоких нагрузках и росте объёма данных.
3. **Безопасность и надежность**: потокобезопасность, отмена операций через `CancellationToken`, graceful degradation.
4. **Совместимость с Native AOT**: отсутствие неявного reflection и динамической генерации типов.

---

## 11. 🧭 Семантическая навигация и граф кода (CodeGraph)

- Для исследования структуры проектов, поиска связей классов, вызовов и анализа влияния (blast radius) в кодовой базе используй семантические инструменты **CodeGraph** (`codegraph_explore`, `codegraph_node`, `codegraph_callers`, `codegraph_impact`).
- Текстовый поиск (`grep_search`) используй для поиска строковых литералов, конфигураций (`appsettings.json`), XML/CSPROJ свойств и файлов локализации.
- Подробности: [rules/codegraph-navigation.md](rules/codegraph-navigation.md).

---

## 12. 👥 Ролевая структура специализированных AI-агентов

Все субагенты экосистемы ориентированы на свои специализированные домены:

| Роль агента | Идентификатор | Срез файловой структуры (`src/`) | Ключевая зона ответственности |
| :--- | :--- | :--- | :--- |
| **Backend Architect** | `backend-architect` | `src/Backend/`, `src/Shared/SharedKernel/` | ScyllaDB NoSQL (zero EF Core), gRPC Protobuf, Kafka, Redis, Native AOT. |
| **Frontend UX Specialist** | `frontend-ux-specialist` | Разметка `*.razor` в `src/Frontend/`, `Material.Web/` | Experience & Structure: чистая разметка, HTML5 семантика, Material.Web first, unquoted синтаксис `что-то=@*`, 3-way .resx parity. |
| **Frontend UI Specialist** | `frontend-ui-specialist` | `src/Frontend/Platform/Shared/DesignSystem/Assets/wwwroot/css/` | Interface & Presentation: стили в `Assets`, токены M3X, `@layer` + изоляция по уникальному корневому атрибуту, pure CSS Grid, touch targets $\ge 48\text{px}$. |
| **Frontend IX Specialist** | `frontend-ix-specialist` | Код `*.razor.cs` и `Features/` в `src/Frontend/` | Interaction & Behavior: Clean Architecture + Vertical Slice (`Features/UseCases`), слои `Pages/Widgets/Components`, чистый C# code-behind, `using static` (0 алиасов), `PersistentComponentState`, жизненный цикл. |

