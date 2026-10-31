# Архитектура фронтенда: 1. Разметка в *.razor файлах

## 1. 🏛️ Золотой стандарт: Чистая разметка и типизированные константы

Файлы с расширением `*.razor` предназначены **исключительно для декларативной разметки дерева пользовательского интерфейса**.
Вся верстка строится на семантическом HTML5, готовых компонентах дизайн-системы `Material.Web` и строго коротких именах констант.

> **ГЛАВНЫЙ ЗАКОН РАЗМЕТКИ**:
> В `.razor` файлах разметки не должно быть сырых строковых литералов, длинных путей к константам, fallback-заглушек и тяжелых блоков бизнес-логики `@code`. Вся C#-логика, параметры и обработчики выносятся в парный файл `*.razor.cs`.

---

## 2. 🔤 Обязательный `@using static` для ВСЕХ констант (Только короткие имена)

Все константы классов, иконок, локализации и статических ассетов подключаются в начале `.razor` файла через лаконичные директивы `@using static` (благодаря `@using Assets.Constants` в `_Imports.razor`):

| Группа констант / Enums | Директива подключения | Использование в разметке | Пример в шаблоне |
|---|---|---|---|
| **CSS-классы** | `@using static Classes.<Group>` | `@<ClassName>` | `<section class=@PanelHeader>` или `class=@($"{Card} {Elevated}")` |
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
- `class=@HeaderBar` (вместо `class="@HeaderBar"`)
- `Variant=@Outlined` (вместо `Variant="ButtonVariant.Outlined"` или `Variant=@ButtonVariant.Outlined`)
- `Variant=@Filled` (вместо `Variant="Filled"` или `Variant=@ButtonVariant.Filled`)
- `Selected=@(durationMonths == 1)` (вместо `Selected="@(durationMonths == 1)"`)
- `Disabled=@(!CanSubmit)` (вместо `Disabled="@(!CanSubmit)"`)
- `LeadingIcon=@Payment` (вместо `LeadingIcon="@Payment"`)
- `Label=@L[LabelName]` (вместо `Label="@L[LabelName]"`)
- `Value=@Val` (вместо `Value="@Val"`)

### Стандарт ультра-короткого синтаксиса: `что-то=@*` или `*=@*`
В разметке `*.razor` все атрибуты записываются в чистом ультра-коротком формате: `что-то=@*` (или `*=@*`).

- **Канонический синтаксис**: `class=@ItemUnitPrice`, `class=@HeaderTitleBox`, `class=@TotalRow`, `Icon=@Close`, `Variant=@Outlined`, `Label=@L[StepCart]`.
- **Импорт через `using static`**: короткие имена подключаются напрямую директивами `@using static Classes.<Group>`, `@using static Icons`, `@using static Loc.<Domain>`, `@using static <EnumName>` в заголовке файла.
- **Без полей-алиасов в code-behind**: константы дизайн-системы не дублируются в `*.razor.cs` (`public const string Alias = ...`), а подключаются через `using static`.
- **Без составных путей с точками**: составные выражения вида `class=@Classes.Common.ItemUnitPrice`, `Icon=@Icons.Close`, `Variant=@ButtonVariant.Outlined` заменяются на ультра-короткие `class=@ItemUnitPrice`, `Icon=@Close`, `Variant=@Outlined`.
- **Разрешение коллизий без точек в атрибутах**: если две группы содержат одноименные константы, импортируйте профильную группу в заголовке файла, сохраняя ультра-короткий синтаксис в атрибуте (`class=@HeaderTitleBox`).

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
3. **Составные классы (Тотальная минимизация кавычек через `string.Format`)**:
   - Для объединения нескольких CSS-классов без кавычек используется метод `string.Format(Temp, ...)`:
     `const string Temp = "{0} {1} {2}";` (в code-behind или через `@using static Formats` / `Two`, `Three`, `Temp`)
     В разметке: `class=@string.Format(Temp, Const1, Const2, Const3)` или `class=@string.Format(Two, Card, Elevated)`.
   - В отличие от интерполяции `class=@($"{Card} {Elevated}")`, вызов `class=@string.Format(Two, Card, Elevated)` **не содержит ни одной двойной кавычки `"`**, гарантируя 100% Zero-Quote синтаксис разметки.
4. **Именование методов-обработчиков (Zero Collisions)**: методы в `*.razor.cs` именуются полно и описательно (`CloseDialogAsync`, `SubmitOrderAsync`), чтобы исключить коллизии со статическими именами иконок (`Icons.Close`).

---

## 4. 🔄 Канонические трансформации паттернов разметки (Антипаттерн ➔ Стандарт)

| Антипаттерн | Канонический стандарт | Инженерное обоснование |
|---|---|---|
| `Icon="@Close"`, `OnClick="Close"`, `class="@Header"` | `Icon=@Close`, `OnClick=@CloseDialogAsync`, `class=@Header` | Чистый синтаксис без кавычек исключает путаницу C#-выражений со строками |
| `class=@Classes.Common.ItemUnitPrice`, `Icon=@Icons.Close` | `class=@ItemUnitPrice`, `Icon=@Close` через `@using static` | Ультра-короткий синтаксис без точек делает разметку чистой и читаемой |
| `Variant=@ButtonVariant.Outlined`, `Type=@ChipType.Filter` | `Variant=@Outlined`, `Type=@Filter` через `@using static <Enum>` | Краткие имена вариантов перечислений без длинных префиксов |
| Поля-алиасы в code-behind (`public const string Alias = ...`) | Прямой `@using static` в `.razor` / `using static` в `.razor.cs` | Исключает дублирование констант и засорение C#-классов |
| Излишняя вложенность (`div > div > div`) | Плоское дерево, выравнивание через CSS Grid на родителе | Облегчает DOM-дерево, ускоряет рендеринг и исключает паразитные обертки |
| Кликабельные `<div>`, `<span>` или тег `<button>` | Компоненты `<Button>` или `<IconButton>` из `Material.Web` | Соответствие Material Design 3 и поддержка доступности (A11y) |
| HTML-теги `<select>`, `<hr>`, `<input>` | Компоненты `<Select>`, `<Divider>`, `<TextField>`, `<Checkbox>` | Единая дизайн-система и валидация на уровне Blazor |
| `<div>` вместо семантики (`<div class="header">`) | `<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<nav>` | Семантическая структура для скринридеров и доступности |
| Строковые литералы вида `"любой текст"` | `@L[Key]` через словари локализации | Поддержка мультиязычности (RU, UZ, EN) без хардкода |
| `class="m3-card elevated"` или `class=@($"{A} {B}")` | `class=@HeaderBar` или `class=@string.Format(Two, Card, Elevated)` | Тотальное исключение двойных кавычек в разметке и типобезопасность классов |
| `@Classes.Cards.Card` или `@Classes.Common.Header` | `@Card`, `@Header` через `@using static Classes.<Group>` | Лаконичная разметка без длинных префиксов |
| `L["HeaderTitle"]` (сырые строки ключей) | `@L[HeaderTitle]` через `@using static Loc.<Domain>` | Проверка ключей компилятором при рефакторинге |
| `@(L[Title] ?? "Заголовок")` (fallback `??`) | `@L[Title]` без заглушек | Чистый вызов локализации, гарантированный parity в `.resx` |
| `<style>...</style>` и инлайн `style="..."` | Стили в `Assets/wwwroot/css/` через `:where(...)` | Консолидация стилей и эффективное Brotli-сжатие бандла |
| Блоки `@code { ... }` с логикой (> 5 строк) | C#-код в парном файле `*.razor.cs` | Разделение ответственности и поддержка Native AOT |

---

## 5. 🧩 Иерархия UI Компонентов и обязательная замена HTML-тегов (Blazor Component First)

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
1. **Устранение "Div Soup"**:
   - Запрещены промежуточные контейнеры-обертки (`<div class="wrapper"><div class="container"><div class="inner">...</div></div></div>`).
   - Благодаря **CSS Grid**, родительский семантический элемент (`<section>`, `<article>`, `<header>`, `<footer>`) напрямую управляет раскладкой, отступами и позиционированием своих дочерних элементов через `display: grid`, `gap`, `grid-template-columns: minmax(0, 1fr)`.
2. **Запрет оберток ради стилей**:
   - Нельзя создавать `<div>` исключительно ради `padding`, `margin`, рамки или фона. Классы оформления навешиваются на сам семантический тег или его непосредственных потомков.
3. **Прямое связывание компонентов**:
   - Размещайте компоненты `Material.Web` (`<Button>`, `<TextField>`) и семантические элементы непосредственно в сетке без промежуточных контейнеров-пустышек.

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
@using static Classes.Cards
@using static Classes.Buttons
@using static Classes.Typography
@using static Classes.Payment
@using static Icons
@using static Loc.Checkout
@using static ButtonVariant
@inject IStringLocalizer L

<article class=@($"{CheckoutPanel} {Card} {Elevated}")>
    <header class=@PanelHeader>
        <h3 class=@($"{Headline} {TitleLarge}")>
            <Icon Name=@AccountBalanceWallet />
            @L[HeaderTitle]
        </h3>
        <p class=@($"{BodySubtext} {BodyMedium}")>@L[HeaderSubtitle]</p>
    </header>

    <nav class=@WorkflowStepper aria-label=@L[StepPayment]>
        <ol class=@StepList>
            <li class=@($"{StepItem} completed")>
                <span class=@StepNum>✓</span>
                <span class=@Label>@L[StepCart]</span>
            </li>
            <li class=@($"{StepLine} active") aria-hidden="true"></li>
            <li class=@($"{StepItem} active") aria-current="step">
                <span class=@StepNum>2</span>
                <span class=@Label>@L[StepPayment]</span>
            </li>
        </ol>
    </nav>

    <footer class=@ActionsGroup>
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
1. **Чистые короткие имена и тотальное сокращение через `@using static` (Ultra-Short Syntax)**: Константы классов (`@Card`), иконок (`@Add`), локализации (`@L[Key]`), ассетов (`@LogoSvg`) и перечислений (`Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`) подключены строго через короткие `@using static Classes.<Group>`, `@using static Icons`, `@using static Loc.<Domain>`, `@using static ButtonVariant`. Запрещены длинные квалификаторы `@ButtonVariant.*`, `@ChipType.*`, `@Classes.*`, `@L[Loc.*]`.
2. **Синтаксис атрибутов без кавычек и ультра-короткий формат `что-то=@*` (0 составных путей с точками `что-то=@*.*.*`)**: Для всех C#-констант, параметров компонентов, методов-обработчиков, классов, булевых флагов и перечислений используется чистый синтаксис без кавычек (`Icon=@Close`, `OnClick=@CloseDialogAsync`, `class=@HeaderBar`, `Open=@IsOpen`, `Variant=@Outlined`). Категорически запрещены любые выражения с точками вида `что-то=@*.*.*` (строго `что-то=@*`); категорически запрещены поля-алиасы в code-behind (только `using static`). Строковые кавычки остаются только для литеральных HTML-атрибутов (`Slot="icon"`). Выражения с дженериками экранируются круглыми скобками `Variant=@(...)`.
3. **Плоская семантическая структура (Zero Wrapper Bloat)**: Использованы правильные теги HTML5 (`<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<aside>`, `<nav>`, `<fieldset>`, `<ul>`/`<li>`). Отсутствует лишняя вложенность дерева; исключены паразитные `<div>` ради отступов; запрещены кликабельные `<div>`/`<span>` с `@onclick`.
4. **Zero Hardcoded Strings**: Никаких строковых констант вида `"любой текст"` в разметке. Весь UI-текст выносится строго в `.resx` (`@L[Key]`) либо в типизированные C#-константы.
5. **Zero Magic Strings**: Ни одной сырой строки `class="..."` и ни одного строкового ключа `L["..."]`.
6. **Zero Prefixes**: Отсутствуют длинные квалификаторы `@Classes.*` и `@L[Loc.*]`.
7. **Zero Fallback Clutter**: Ни одного `?? "..."` в вызовах локализации.
8. **Material.Web First**: Использованы готовые компоненты `Material.Web` (`<Button>`, `<TextField>`, `<MaterialDialog>`).
9. **Zero Style Clutter**: Ни одного блока `<style>` и ни одного инлайн-стиля `style="..."`.
10. **Zero Code Clutter**: В файле разметки нет тяжелых блоков `@code` с логикой (вся C#-логика вынесена в `*.razor.cs`).
11. **3-Way Localization**: Все используемые ключи присутствуют в `ResourceRu.resx`, `ResourceUz.resx` и `ResourceEn.resx`.
12. **Touch Targets & A11y**: Все интерактивные элементы имеют размер не менее 48x48px и понятные подписи / `aria-label`.
