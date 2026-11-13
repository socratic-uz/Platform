# Архитектура фронтенда: 1. Разметка в *.razor файлах

## 1. 🏛️ Золотой стандарт: Чистая разметка и типизированные константы

Файлы с расширением `*.razor` предназначены **исключительно для декларативной разметки дерева пользовательского интерфейса**.
Вся верстка строится на семантическом HTML5, готовых компонентах дизайн-системы `Material.Web` и строго коротких именах констант.

> **ГЛАВНЫЙ ЗАКОН РАЗМЕТКИ**:
> В `.razor` файлах разметки не должно быть сырых строковых литералов, длинных путей к константам, fallback-заглушек и тяжелых блоков бизнес-логики `@code`. Вся C#-логика, параметры и обработчики выносятся в парный файл `*.razor.cs`.

### 1.1. ⚡ Тотальный отказ от CSS-классов (Pure Attribute-Driven Classless)
В файлах `*.razor` **строго запрещено** использовать CSS-классы (`class=@...`) на семантических HTML5-элементах.
- **Трёхуровневая модель навигации и селекторов**:
  1. **Уровень документа (`body`)**: единый универсальный 3-колоночный лейаут на самом теге `body` (`grid-template-columns: auto minmax(0, 1fr) auto;`). Адаптивность переключается через `grid-template-areas`.
  2. **Уровень страницы (`landmarks первого уровня DOM`)**: `header[page]`, `nav[page]`, `main[page]`, `footer[page]`. Например: `<header kiosk>`, `<main kiosk>`, `<footer kiosk>`. Их стили пишутся напрямую: `header[kiosk] > tag1 > tag2`, `main[kiosk] > tag1 > tag2`, `footer[kiosk] > tag1 > tag2`. Запрещено писать `body:has(main[kiosk]) header...` — `header[kiosk]` сам является ориентиром 1-го уровня.
  3. **Уровень виджетов (`root-tag первого уровня внутри виджета`)**: `root-tag[widget] > child-tag > child-child-tag` (например: `<aside cart>`, `<aside order>`, `<dialog modal>`).
- **Атрибут ТОЛЬКО на первом уровне DOM дерева**: Атрибут указывается строго на корневом теге компонента/виджета/ориентира страницы (`header[kiosk]`, `main[kiosk]`, `footer[kiosk]`, `aside[cart]`, `aside[order]`, `dialog[modal]`). Дочерние узлы внутри контейнера НЕ несут вспомогательных атрибутов.
- Дочерние элементы являются чистыми семантическими HTML5-тегами и стилизуются по прямому дереву:
  `[card] > figure > img`
  `main[kiosk] > section > article`
  `aside[cart] > header > hgroup > h3`
  `aside[cart] > ul > li`
  `aside[cart] > footer > dl > p`
- Директивы `@using static Classes.*` и класс-константы выводятся из эксплуатации.

### 1.2. 🚫 Запрет `@page` и `@attribute` в разметке (*.razor)

В файлах `*.razor` **категорически запрещено** использовать директивы маршрутизации и безопасности:
- ❌ `@page "/..."`
- ❌ `@attribute [Authorize]`
- ❌ `@attribute [Authorize(Policy = "...")]`
- ❌ `@attribute [AllowAnonymous]`

**Правило**: Разметка должна оставаться чистой декларативной структурой DOM без инфраструктурных метаданных.
Вся конфигурация маршрутизации и авторизации страниц выносится строго в парный файл `*.razor.cs` через стандартные C#-атрибуты над частичным классом:
- `[Route("/...")]` (из неймспейса `Microsoft.AspNetCore.Components`)
- `[Authorize]` / `[AllowAnonymous]` (из неймспейса `Microsoft.AspNetCore.Authorization`)

### 1.3. 📄 Обязательный файл спецификаций страницы (`*.razor.md`)

Каждая страница в `Pages/` сопровождается парным файлом спецификаций `PageName.razor.md`, который описывает её бизнес-контракт и архитектуру:
1. **Назначение и бизнес-возможности** — какую задачу решает страница, для какой роли (Покупатель / Продавец / Оператор / Система) и какие бизнес-возможности предоставляет.
2. **Паспорт маршрута и доступа** — `[Route(...)]`, `[Authorize]` / публичный доступ, SEO/метаданные (`<HeadContent>`, Soft-404 при необходимости).
3. **Лейаут, ориентиры и панели (`aside.md`)** — уникальный корневой атрибут страницы (`<main ...>`), наличие `<Aside>`, `<Header>`, `<Footer>`, `<Fab>`, всплывающие/встроенные панели и триггеры.
4. **Виджеты, компоненты и стили** — файл стилей `pages/*.css`, используемые виджеты (`widgets/*.css`) и ключевые компоненты.
5. **Поведение, UseCases и состояния (IX)** — основные сценарии взаимодействия, инжектируемые сервисы/UseCases, обработка состояний загрузки, пустого результата и ошибок.

---

## 2. 🔤 Обязательный `@using static` для ВСЕХ констант (Только короткие имена)

Все константы иконок, локализации, перечислений и статических ассетов подключаются в начале `.razor` файла через лаконичные директивы `@using static` (благодаря `@using Assets.Constants` в `_Imports.razor`):

| Группа констант / Enums | Директива подключения | Использование в разметке | Пример в шаблоне |
|---|---|---|---|
| **Иконки** | `@using static Icons` | `@<IconName>` | `<Icon Name=@AccountBalanceWallet />` |
| **Локализация** | `@using static Loc.<Domain>` | `@L[<KeyName>]` | `<h3>@L[HeaderTitle]</h3>` или `<TextField Label=@L[HeaderTitle] />` |
| **Ассеты / URL** | `@using static Assets.<Group>` | `@<AssetName>` | `<img src=@LogoSvg alt=@L[LogoAlt] />` |
| **Перечисления (Enums)** | `@using static <EnumName>` | `@<EnumValue>` | `<Button Variant=@Outlined />`, `<Chip Type=@Filter />` |

---

## 3. ⚡ Чистый синтаксис атрибутов без кавычек (Unquoted Razor Syntax)

Для всех параметров Blazor-компонентов, обработчиков событий, иконок, классов, перечислений (enums), булевых условий и чисел **строго используется синтаксис без кавычек**:

- `Icon=@Close` (вместо `Icon="@Close"`)
- `OnClick=@CloseDialogAsync` (вместо `OnClick="CloseDialogAsync"` или `OnClick="@CloseDialogAsync"`)
- `Open=@IsOpen` (вместо `Open="@IsOpen"`)
- `Variant=@Outlined` (вместо `Variant="ButtonVariant.Outlined"` или `Variant=@ButtonVariant.Outlined`)
- `Variant=@Filled` (вместо `Variant="Filled"` или `Variant=@ButtonVariant.Filled`)
- `Selected=@(durationMonths == 1)` (вместо `Selected="@(durationMonths == 1)"`)
- `Disabled=@(!CanSubmit)` (вместо `Disabled="@(!CanSubmit)"`)
- `LeadingIcon=@Payment` (вместо `LeadingIcon="@Payment"`)
- `Label=@L[LabelName]` (вместо `Label="@L[LabelName]"`)
- `Value=@Val` (вместо `Value="@Val"`)

### Стандарт ультра-короткого синтаксиса: `что-то=@*` или `*=@*`
В разметке `*.razor` все атрибуты записываются в чистом ультра-коротком формате: `что-то=@*` (или `*=@*`).

- **Канонический синтаксис**: `Icon=@Close`, `Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`, `Label=@L[StepCart]`, `Selected=@IsActive`.
- **Импорт через `using static`**: короткие имена подключаются напрямую директивами `@using static Icons`, `@using static Loc.<Domain>`, `@using static <EnumName>` в заголовке файла.
- **Без полей-алиасов в code-behind**: константы дизайн-системы не дублируются в `*.razor.cs` (`public const string Alias = ...`), а подключаются через `using static`.
- **Без составных путей с точками**: составные выражения вида `Icon=@Icons.Close`, `Variant=@ButtonVariant.Outlined`, `Label=@Loc.Checkout.StepCart` заменяются на ультра-короткие `Icon=@Close`, `Variant=@Outlined`, `Label=@L[StepCart]`.
- **Разрешение коллизий без точек в атрибутах**: если две группы содержат одноименные константы, импортируйте профильную группу в заголовке файла, сохраняя ультра-короткий синтаксис в атрибуте.

### Тотальное сокращение перечислений через `@using static <EnumName>`:
Для всех перечислений (enums), типов вариантов и фабрик используется директива `@using static`:
- `Variant=@Outlined` или `Variant=@Filled` ➔ через `@using static ButtonVariant`
- `Type=@Filter` ➔ через `@using static ChipType`
- `Variant=@Standard` ➔ через `@using static IconButtonVariant`

### Особые правила синтаксиса и парсера Razor:
1. **Литеральные строки HTML и слоты веб-компонентов** остаются строковыми атрибутами HTML в кавычках: `Slot="icon"`, `type="button"`, `role="dialog"`.
2. **Дженерики в атрибутах оборачиваются в скобки** `Variant=@(...)`, чтобы Razor-парсер не спутал `<T>` с открывающим HTML-тегом:
   - `Variant=@(TextField<decimal>.TextFieldVariant.Outlined)`
   - `Format=@(NumberFormat<decimal>.Currency)`
4. **Именование методов-обработчиков (Zero Collisions)**: методы в `*.razor.cs` именуются полно и описательно (`CloseDialogAsync`, `SubmitOrderAsync`), чтобы исключить коллизии со статическими именами иконок (`Icons.Close`).

---

## 4. 🔄 Канонические трансформации паттернов разметки (Антипаттерн ➔ Стандарт)

| Антипаттерн | Канонический стандарт | Инженерное обоснование |
|---|---|---|
| `Icon="@Close"`, `OnClick="Close"` | `Icon=@Close`, `OnClick=@CloseDialogAsync` | Чистый синтаксис без кавычек исключает путаницу C#-выражений со строками |
| `Icon=@Icons.Close`, `Variant=@ButtonVariant.Outlined` | `Icon=@Close`, `Variant=@Outlined` через `@using static` | Ультра-короткий синтаксис без точек делает разметку чистой и читаемой |
| `Variant=@ButtonVariant.Outlined`, `Type=@ChipType.Filter` | `Variant=@Outlined`, `Type=@Filter` через `@using static <Enum>` | Краткие имена вариантов перечислений без длинных префиксов |
| Поля-алиасы в code-behind (`public const string Alias = ...`) | Прямой `@using static` в `.razor` / `using static` в `.razor.cs` | Исключает дублирование констант и засорение C#-классов |
| Излишняя вложенность (`div > div > div`) | Плоское дерево, выравнивание через CSS Grid на родителе | Облегчает DOM-дерево, ускоряет рендеринг и исключает паразитные обертки |
| Кликабельные `<div>`, `<span>` или тег `<button>` | Компоненты `<Button>` или `<IconButton>` из `Material.Web` | Соответствие Material Design 3 и поддержка доступности (A11y) |
| HTML-теги `<select>`, `<hr>`, `<input>` | Компоненты `<Select>`, `<Divider>`, `<TextField>`, `<Checkbox>` | Единая дизайн-система и валидация на уровне Blazor |
| `<div>` вместо семантики (`<div class="header">`) | `<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<nav>` | Семантическая структура для скринридеров и доступности |
| Строковые литералы вида `"любой текст"` | `@L[Key]` через словари локализации | Поддержка мультиязычности (RU, UZ, EN) без хардкода |
| `class="m3-card elevated"` или `class=@...` | Pure Attribute-Driven Classless: семантические HTML5-теги и атрибуты 1-го уровня (`[card]`, `main[kiosk]`, `aside[cart]`) | Полный отказ от классов, стили от 1-го уровня DOM |
| `L["HeaderTitle"]` (сырые строки ключей) | `@L[HeaderTitle]` через `@using static Loc.<Domain>` | Проверка ключей компилятором при рефакторинге |
| `@(L[Title] ?? "Заголовок")` (fallback `??`) | `@L[Title]` без заглушек | Чистый вызов локализации, гарантированный parity в `.resx` |
| `<style>...</style>` и инлайн `style="..."` | Стили в `Assets/wwwroot/css/` по слоёной архитектуре Cascade Layers (`@layer`) | Чистая разметка и предсказуемый каскад стилей |
| Блоки `@code { ... }` с логикой (> 5 строк) | C#-код в парном файле `*.razor.cs` | Разделение ответственности и поддержка Native AOT |

---

## 5. 🏛️ Архитектура ориентиров страниц (Page Landmarks) и Контракт растягивания/центрирования

### 5.1. Единый универсальный лейаут body
- **Единая 3-колоночная макросетка на теге `<body>`**:
  Каркас раскладки ориентиров (`body > header`, `body > aside:has(~ main)`, `body > main`, `body > footer`, `body > main ~ aside`) задается исключительно CSS Grid на теге `body` (`grid-template-columns: auto minmax(0, 1fr) auto;`).
- **Ликвидация служебного тега `<layout>`**:
  Механизм переключения через `<layout [variant]>` упразднен в пользу единого универсального адаптивного лейаута.
- **Булевы атрибуты ориентиров страниц (Boolean Page Flags)**:
  Прямые семантические ориентиры экрана (`<header>`, `<nav>`, `<main>`, `<footer>`) помечаются **строго булевым атрибутом страницы**:
  `<main kiosk>`, `<main terminal>`, `<main orders>`, `<main payment>`, `<main about>`, `<header about>`.
  - **Категорически запрещены атрибуты со значениями**: `[page="kiosk"]` запрещен, строго `[kiosk]`.
  - **Ориентиры не несут атрибутов лейаута**: `<main grid>` запрещен.
  - **Zero Widget Leakage**: внутри `.razor` страниц виджеты размещаются параллельно; внутренняя верстка виджетов стилизуется в `@layer widgets`.

### 5.3. Архитектура ориентиров `body > aside` (Двух-Aside модель, Панели и Мобильные Триггеры)
Ориентиры документа `body > aside` служат **адаптивными зонами сетки** для навигации и модальных/встроенных поверхностей Material Design 3.

#### 1. Двух-Aside модель (Two Asides Model)
- **Первичный aside (`body > aside:not(main ~ aside)`)**:
  - Содержит `<nav>` — постоянную навигацию: Navigation Rail (`76px`) слева на Desktop; Navigation Bar (`60px`) внизу на Mobile.
  - Содержит всплывающие панели (`md-dialog` / `<MaterialDialog>`):
    - На Desktop: левая панель (`Вспл. лев.`) открывается от самого левого края экрана (`left: 0; z-index: 1001;`), перекрывая 76px рельс навигации; правая панель (`Вспл. прав.`, Корзина Киоска) открывается справа (`right: 0; z-index: 1001;`).
    - На Mobile: левая и правая панели занимают **всю высоту экрана** (`top: 0; bottom: 0; height: 100dvh; z-index: 1001;`), полностью перекрывая нижний бар навигации (`z-index: 90`).
    - Нижняя панель (`Вспл. ниж.`, Профиль) открывается от `bottom: 0` поверх нижнего бара со скруглением верхних углов `28px 28px 0 0`.
- **Вторичный aside (`body > main ~ aside`)**:
  - Встроенная панель на Desktop (`[docked]`, `Встр. прав.`, 3-я колонка макросетки `body`, например чек терминала `Terminal`, боковик чата `ChatPage`).
  - На Mobile встроенные панели **отсутствуют**: все второстепенные панели превращаются во всплывающие правые шторки (`Вспл. прав.`), вызываемые через FAB в правом нижнем углу `main`.

#### 2. Эргономическая триада триггеров на мобильных устройствах (< 840px)
- ☰ **Левый угол `header`** ➔ **`Вспл. лев.`** (Левая шторка: Главное меню навигации, AI чаты, Документация) — на всю высоту экрана `100dvh`.
- 👤 **Правый угол `header` (Аватар)** ➔ **`Вспл. ниж.`** (Нижняя шторка: Профиль пользователя, поиск карты) — снизу экрана (`bottom: 0`).
- ⚡ **Правый нижний угол `main` (FAB)** ➔ **`Вспл. прав.`** (Правая шторка: Корзина, Чек, Параметры, Инструменты, QR, Сводка, Конструкторы) — на всю высоту экрана `100dvh`.

#### 3. Архитектура скрима (`#sheet-scrim`) и Взаимное исключение (Mutual Exclusion)
- **Взаимное исключение**: в один момент времени может быть открыта только **одна** всплывающая модальная панель (открытие новой автоматически закрывает предыдущую).
- **Скрим затемнения (`#sheet-scrim`)**:
  - Полноэкранный скрим (`z-index: 1000`, `inset: 0`) монтируется внутри активного `<aside>` перед `<md-dialog>`, исключая ловушки изолированных контекстов наложения (Stacking Context).
  - Скрим присутствует для всех всплывающих панелей (`Вспл. лев.`, `Вспл. прав.`, `Вспл. ниж.`).
  - Скрим отсутствует для встроенных панелей на десктопе (`[docked]`) и элементов управления картой (`[map-controls]`).
- **Чистое именование**: полный отказ от префикса `socratic-` в именах keyframes (`slide-in-right`, `slide-in-left`, `slide-in-bottom` и парные `slide-out-*`), ID (`#sheet-scrim`) и DOM-атрибутах (`data-aside-sheet`, `data-closing`).

### 5.4. Растягивание на всю область (Full Width by Default) и центрирование (Opt-in Centering)
- **По умолчанию элементы занимают всю доступную область**:
  Все страницы, секции и контейнеры по умолчанию растягиваются на 100% доступной ширины (`width: 100%; min-width: 0`). Запрещены глобальные искусственные `max-width: 1200px` и `margin: 0 auto`.
- **Центрирование подключается только явно через семантические атрибуты**:
  Если конкретному блоку или экрану (например, карточке входа или пустому состоянию) требуется центрирование, оно задаётся строго декларативно:
  - `<tag center>` ➔ `place-content: center; place-items: center;`
  - `<tag center-h>` ➔ `justify-content: center; justify-items: center;`
  - `<tag center-v>` ➔ `align-content: center; align-items: center;`
  - `<tag center-self>` ➔ `justify-self: center; margin-inline: auto;`
  - `<tag center-text>` ➔ `text-align: center;`

---

## 6. 🧩 Иерархия UI Компонентов и обязательная замена HTML-тегов (Blazor Component First)

> **КРИТИЧЕСКОЕ ПРАВИЛО**:
> Если для семантического HTML-тега существует Blazor-компонент дизайн-системы `Material.Web` — **ОБЯЗАТЕЛЬНО использовать Blazor-компонент**.
> Например: **строго `<Button>` (или `<IconButton>`) вместо `<button>`**.

### Таблица обязательных замен семантических тегов:

| HTML тег / элемент | Обязательный Blazor-компонент `Material.Web` | Пример использования |
|---|---|---|
| `<button type="...">` | `<Button>` (или `<IconButton>` для иконок) | `<Button Variant=@Filled LeadingIcon=@Add OnClick=@SaveAsync>` |
| `<button>` с одной иконкой | `<IconButton>` | `<IconButton Icon=@Close OnClick=@CloseDialogAsync AriaLabel=@L[CloseAction] />` |
| `<input type="text/password/email">` / `<textarea>` | `<TextField>` | `<TextField Label=@L[Email] Value=@Email />` |
| `<input type="checkbox">` | `<Checkbox>` | `<Checkbox Checked=@IsActive OnChange=@ToggleActive />` |
| `<input type="radio">` | `<Radio>` | `<Radio Value=@OptionId Name="group" />` |
| `<select>` / `<option>` | `<Select>` / `<SelectOption>` | `<Select Label=@L[Country]>...</Select>` |
| `<hr>` | `<Divider>` | `<Divider />` |
| `<dialog>` | `<MaterialDialog>` или `<Dialog>` | `<MaterialDialog Open=@IsOpen Headline=@L[ModalTitle]>` |
| Drawer / Side Sheet / Bottom Sheet | `<MaterialDialog>` с атрибутом 1-го уровня | `<MaterialDialog drawer>`, `<MaterialDialog side-sheet>`, `<MaterialDialog sheet>` |
| `<progress>` | `<Progress>` | `<Progress Value=@ProgressValue />` |
| Вкладки / Табы | `<Tabs>` + `<Tab>` | `<Tabs><Tab Content=@L[TabOverview] /></Tabs>` |
| Фильтры / Чипы | `<Chip>` + `<ChipSet>` | `<Chip Type=@Filter Label=@L[FilterName] />` |
| Значки / Бейджи | `<Badge>` | `<Badge Value="3">` |
| Карточки сущностей | `<Card>` или `<article>` | `<Card Variant=@Elevated>` |

При построении элементов разметки строго соблюдается приоритет:

1. **Приоритет №1: Готовый Razor-компонент из `Material.Web`** (`src/Frontend/Platform/Shared/DesignSystem/Material.Web/`):
   - Обязательно заменяет стандартные HTML-теги (`<button>` ➔ `<Button>`, `<select>` ➔ `<Select>`, `<hr>` ➔ `<Divider>`).
2. **Приоритет №2: Веб-компонент `@material/web`** (только если в `Material.Web` нет обертки):
   - `<md-filled-button>`, `<md-outlined-text-field>`, `<md-icon>@Search</md-icon>`
3. **Приоритет №3: Семантические HTML5 элементы (Строго если в `Material.Web` НЕТ компонента-аналога)**:

| Семантический тег | Назначение в разметке Socratic | Что заменяет |
|---|---|---|
| `<header>` | Шапка страницы, секции, карточки или модального окна с заголовками и метаданными | Запрещен `<div class="header">` |
| `<footer>` | Подвал, панель кнопок действий диалога, блок подтверждения заказа | Запрещен `<div class="actions">` |
| `<main>` | Основная рабочая область страницы или представления | Запрещен `<div class="main-content">` |
| `<section>` | Независимый тематический раздел, секция формы, этап визарда | Запрещен `<div class="section">` |
| `<article>` | Автономная карточка сущности (товар, лот аукциона, бронь, номер отеля, тариф) | Запрещен `<div class="card">` |
| `<aside>` | Боковая панель, сайдбар корзины, фильтры каталога, панель итогов | Запрещен `<div class="sidebar">` |
| `<nav>` | Навигация, шаги оформления (`stepper`), табы, пагинация, хлебные крошки | Запрещен `<div class="nav-steps">` |
| `<fieldset>` + `<legend>` | Логически связанные группы полей ввода и параметров конфигуратора | Запрещен `<div class="form-group">` |
| `<ul>` / `<ol>` + `<li>` | Списки элементов, фич, опций, позиций чека (семантический список) | Запрещен набор разрозненных `<div>` |
| `<time datetime="...">` | Даты, временные интервалы брони, таймеры обратного отсчета | Запрещен `<span class="time">` |
| `<figure>` + `<figcaption>` | Медиа-превью товара, интерактивная схема рассадки с подписью | Запрещен `<div class="image-box">` |

---

## 6. 📐 Плоское семантическое дерево (Zero Excessive Nesting & Zero Wrapper Bloat)

В разметке компонентов **категорически запрещена избыточная вложенность контейнеров**:

### 6.1. Метрики глубины вложенности (Exact Target Metrics)
- **Целевой стандарт сквозной глубины (Target Depth):** строго **`6` уровней** от `<body>` до целевого элемента интерфейса.
- **Абсолютный потолок (Hard Ceiling):** строго **`7` уровней** (только для конечных листовых атомов: `<Icon>`, `<span>`, `<strong>`, `@L[...]` внутри кнопок и полей ввода).
- **Архитектурный дефект (Violation):** **`8+` уровней** — категорически запрещено. Наличие 8+ уровней в DOM-дереве блокируется при ревью как «Div Soup» либо нарушение компонентной декомпозиции.
- **Локальный лимит файла (`*.razor` Scope):** **$\le 4$ уровней** от корневого тега файла до самого глубокого дочернего элемента.

### 6.2. Иерархия уровней: от Layout до Атома

| Уровень | Слой в CSS (`@layer`) | Архитектурная роль | Допустимые семантические теги / Компоненты |
| :--- | :--- | :--- | :--- |
| **Уровень 1** | `@layer layouts;` | **Макро-каркас приложения** (App Shell) | `<body>`, `<header>`, `<nav>`, `<main>`, `<footer>` |
| **Уровень 2** | `@layer pages;` | **Сетка страницы / экрана** (Page Macro-Grid) | `<main>`, `<section>`, `<header>`, `<nav>`, `<aside>` |
| **Уровень 3** | `@layer widgets;` | **Отраслевой бизнес-организм** (Smart Widget) | `<section>`, `<aside>`, `<fieldset>` + `<legend>`, `<article>`, `<MaterialDialog>` |
| **Уровень 4** | `@layer widgets;` / `@layer components;` | **Внутренняя группировка виджета** (Sub-Grid) | `<ul>` / `<ol>`, `<fieldset grid-2-col>`, `<menu actions>` |
| **Уровень 5** | `@layer components;` | **UI-примитивы и молекулы** (Dumb Components) | `<Button>`, `<IconButton>`, `<TextField>`, `<Select>`, `<Card>`, `<li>`, `<Tabs>` |
| **Уровень 6** | `@layer components;` | **Внутренний слот / контейнер компонента** | `<span>`, `<div slot="...">`, `<strong>` |
| **Уровень 7** | — | **Листовой неделимый атом** (Leaf Atom) | `<Icon Name=@...>`, `<time>`, текст `@L[...]` |

### 6.3. Сквозной эталон 7 уровней (Worst-Case DOM Trace)
```text
Уровень 1 (Root):       <body>                           [Layout: App.razor]
   └── Уровень 2 (Shell):      <main>                    [Layout: GridLayout.razor / @Body]
          └── Уровень 3 (Page):       <section>          [Page: макро-зона PosPage.razor]
                 └── Уровень 4 (Widget):     <aside>     [Widget: корень CartWidget.razor]
                        └── Уровень 5 (Group):      <ul> [Widget Sub-Grid: список позиций]
                               └── Уровень 6 (Comp):    <li> / <Button> [Component: строка / кнопка]
                                      └── Уровень 7 (Atom): <Icon> / @L[...] [Leaf Atom: иконка или текст]
```

### 6.4. Правила ликвидации паразитных обёрток
1. **Устранение "Div Soup"**:
   - Запрещены промежуточные контейнеры-обертки (`<div class="wrapper"><div class="container"><div class="inner">...</div></div></div>`).
   - Благодаря **CSS Grid**, родительский семантический элемент (`<section>`, `<article>`, `<header>`, `<footer>`) напрямую управляет раскладкой, отступами и позиционированием своих дочерних элементов через `display: grid`, `gap`, `grid-template-columns: minmax(0, 1fr)`.
2. **Запрет оберток ради стилей**:
   - Нельзя создавать `<div>` исключительно ради `padding`, `margin`, рамки или фона. Классы оформления навешиваются на сам семантический тег или его непосредственных потомков.
3. **Прямое связывание компонентов**:
   - Размещайте компоненты `Material.Web` (`<Button>`, `<TextField>`) и семантические элементы непосредственно в сетке без промежуточных контейнеров-пустышек.
4. **Матрица ликвидации обёрток (Zero Wrapper Alternatives)**:

| Зачем разработчик ставил <div> | Антипаттерн (<div>) | Канонический стандарт Socratic |
| :--- | :--- | :--- |
| **2–3 колонки полей ввода** | <fieldset> <div class="grid-2col"> <input/> </div> </fieldset> | <fieldset grid-2-col> <legend>...</legend> <TextField/> </fieldset> (сетка на самом <fieldset>) |
| **Ряд кнопок действий** | <div class="action-buttons"> <Button/> </div> | <menu actions> или <footer actions> |
| **Группа заголовка и иконки** | <div class="title-box"> <Icon/> <h3>...</h3> </div> | <header> с grid-auto-flow или <hgroup> |
| **Скроллируемый список** | <div class="scroll-box"> <ul>...</ul> </div> | <ul scrollable> (прямой overflow-y: auto; на <ul>) |
| **Фон/тень/стекло (Glass/Card)** | <div class="glass-card"> <section>...</section> </div> | <section card glass> (семантические атрибуты прямо на теге) |
| **Боковая панель / сайдбар** | <div class="sidebar">...</div> | <aside sidebar>... |
5. **Виджеты — плоские соседи, а не матрёшки**:
   - Бизнес-виджеты размещаются в сетке страницы параллельно, как независимые братья, а не вкладываются один в другой. Любая матрёшечная вложенность виджета в виджет запрещена.

---

## 7. 🌐 Локализация и синхронизация словарей (i18n)

- Все ключи интерфейса хранятся симметрично в 3 файлах `src/Frontend/Platform/Shared/DesignSystem/Assets/Resources/`:
  - `ResourceRu.resx` — Русский (основной)
  - `ResourceUz.resx` — Узбекский (латиница)
  - `ResourceEn.resx` — Английский
- **Симметрия обязательна**: Каждый новый ключ добавляется во все 3 файла одновременно.
- **Генерация констант**: После изменения `.resx` запускается `npm run refresh` в `Assets`, обновляя `Loc.g.cs`.

---

## 8. 📱 Доступность и эргономика (Touch Ergonomics & A11y)

- **Touch Targets**: Все интерактивные элементы (`<Button>`, `<button>`, `<a>`, `<md-icon-button>`) обязаны иметь фактический размер не менее **48x48px** (микро-кнопки в мобильных сценариях, POS-терминалах и киосках запрещены).
- **ARIA & Семантика**: Все иконки без текста сопровождаются `aria-label=@L[ActionName]`. Информационные баннеры размечаются `role="status"` или `role="alert"`.

---

## 9. 📄 Канонический пример чистого файла разметки (*.razor)

```razor
@namespace Socratic.Checkout
@using static Icons
@using static Loc.Checkout
@using static ButtonVariant
@inject IStringLocalizer L

<article panel-checkout>
    <header>
        <h3>
            <Icon Name=@AccountBalanceWallet />
            @L[HeaderTitle]
        </h3>
        <p>@L[HeaderSubtitle]</p>
    </header>

    <nav stepper aria-label=@L[StepPayment]>
        <ol>
            <li completed>
                <span>✓</span>
                <span>@L[StepCart]</span>
            </li>
            <li line active aria-hidden="true"></li>
            <li active aria-current="step">
                <span>2</span>
                <span>@L[StepPayment]</span>
            </li>
        </ol>
    </nav>

    <footer actions>
        <Button Variant=@Filled LeadingIcon=@Payment OnClick=@ProcessPaymentAsync>
            @L[PayButton]
        </Button>
        <Button Variant=@Outlined LeadingIcon=@Close OnClick=@CancelAsync>
            @L[CancelButton]
        </Button>
    </footer>
</article>
```

---

## 10. ✅ Definition of Done (DoD) для разметки (*.razor)

Перед сдачей любого Razor-компонента или страницы агент обязан проверить:
1. **Чистые короткие имена и тотальное сокращение через `@using static` (Ultra-Short Syntax)**: Константы иконок (`@Add`, `@Close`), локализации (`@L[Key]`), ассетов (`@LogoSvg`) и перечислений (`Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`) подключены строго через короткие директивы `@using static Icons`, `@using static Loc.<Domain>`, `@using static ButtonVariant`. Запрещены длинные квалификаторы `@ButtonVariant.*`, `@ChipType.*`, `@L[Loc.*]`. Кодогенераторы классов ликвидированы; директивы `@using static Classes.*` запрещены.
2. **Синтаксис параметров без кавычек и ультра-короткий формат `что-то=@*` (0 составных путей с точками `что-то=@*.*.*`)**: Для всех C#-констант, параметров компонентов, методов-обработчиков, булевых флагов и перечислений используется чистый синтаксис без кавычек (`Icon=@Close`, `OnClick=@CloseDialogAsync`, `Open=@IsOpen`, `Variant=@Outlined`, `Label=@L[Key]`). CSS-классы (`class=@...`) исключены. Категорически запрещены любые выражения с точками вида `что-то=@*.*.*` (строго `что-то=@*`); категорически запрещены поля-алиасы в code-behind (только `using static`). Строковые кавычки остаются только для литеральных HTML-атрибутов (`Slot="icon"`). Выражения с дженериками экранируются круглыми скобками `Variant=@(...)`.
3. **Плоская семантическая структура (Zero Wrapper Bloat)**: Использованы правильные теги HTML5 (`<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<aside>`, `<nav>`, `<fieldset>`, `<ul>`/`<li>`). Отсутствует лишняя вложенность дерева; исключены паразитные `<div>` ради отступов; запрещены кликабельные `<div>`/`<span>` с `@onclick`.
4. **Zero Hardcoded Strings**: Никаких строковых констант вида `"любой текст"` в разметке. Весь UI-текст выносится строго в `.resx` (`@L[Key]`) либо в типизированные C#-константы.
5. **Zero Magic Strings & Zero Classes**: Полное отсутствие атрибутов `class` на HTML5-тегах и ни одного строкового ключа `L["..."]`.
6. **Zero Prefixes**: Отсутствуют длинные квалификаторы вида `@L[Loc.*]` или составные пути с точками в параметрах.
7. **Zero Fallback Clutter**: Ни одного `?? "..."` в вызовах локализации.
8. **Material.Web First**: Использованы готовые компоненты `Material.Web` (`<Button>`, `<TextField>`, `<MaterialDialog>`).
9. **Zero Style Clutter**: Ни одного блока `<style>` и ни одного инлайн-стиля `style="..."`.
10. **Zero Code Clutter**: В файле разметки нет тяжелых блоков `@code` с логикой (вся C#-логика вынесена в `*.razor.cs`).
11. **3-Way Localization**: Все используемые ключи присутствуют в `ResourceRu.resx`, `ResourceUz.resx` и `ResourceEn.resx`.
12. **Touch Targets & A11y**: Все интерактивные элементы имеют размер не менее 48x48px и понятные подписи / `aria-label`.
