---
name: markup-refactor-loop
description: >-
  Data-driven Razor markup refactoring loop for Blazor templates (*.razor) and code-behind (*.razor.cs), enforcing unquoted syntax, Material.Web first, short @using static constants, 3-way .resx parity, and zero magic strings.
---

# Workflow: Razor Markup & Code-Behind Refactoring Loop

Working dir for codegen and analytics: `src/Frontend/Platform/Shared/DesignSystem/Assets`.
Target components reside across `src/Frontend/` (`Platform/`, `Retail/`, `Portal/`, `Intelligence/`, `Studio/`).

---

## 0. Refresh & Audit (Data-Driven Discovery)

Run the analytics and code generators to get updated metrics:

```powershell
# В папке src/Frontend/Platform/Shared/DesignSystem/Assets
npm run refresh
```

Для анализа нарушений по модулям проверяй сгенерированный манифест:
```powershell
# В папке src/Frontend/Platform/Shared/DesignSystem/Assets
Get-Content wwwroot/analytics/manifest.json | ConvertFrom-Json
```

Источники истины (JSON-аналитика):
- `wwwroot/analytics/usage/class-constants.json` → сырые классы, требующие типизированных констант.
- `wwwroot/analytics/usage/localization.json` → сырые строковые ключи `L["..."]` и незарегистрированные строки.
- `wwwroot/analytics/usage/components.json` → сырые компоненты `md-*`, где доступен компонент `Material.Web`.
- `wwwroot/analytics/manifest.json` → страницы с `totalViolations > 0`.

---

## 1. Выбор цели (Pick Target from Data)

Выбирай компоненты строго по количеству нарушений (от худшего к лучшему):
1. **Приоритет нарушений**:
   - Захардкоженный текст (сырые русские/английские строки в разметке) ➔ перенос в 3 словаря `.resx` (`@L[Key]`).
   - Кавычки вокруг C#-выражений в атрибутах (`Icon="@Close"`, `OnClick="Close"`, `class="@Header"`) ➔ синтаксис без кавычек `Icon=@Close`, `OnClick=@CloseDialogAsync`, `class=@HeaderBar`.
   - Длинные выражения с точками в атрибутах (`что-то=@*.*.*`) ➔ ультра-короткий синтаксис `что-то=@*` строго через `@using static` (без создания алиасов в code-behind).
   - Сырые строки классов `class="..."` и длинные префиксы `@Classes.*` ➔ `@using static Classes.<Group>` + `@ClassName`.
   - Сырые иконки `Icon="close"` или `<md-icon>close</md-icon>` ➔ `@using static Icons` + `Icon=@Close`.
   - Сырые компоненты `<md-filled-button>` ➔ готовые Razor-компоненты `Material.Web` (`<Button Variant=@Filled>`).
   - Длинные квалификаторы перечислений (`@ButtonVariant.*`, `@ChipType.*`) ➔ ультра-короткий синтаксис через `@using static <Enum>` (`Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`).
   - Fallback-заглушки вида `?? "..."` ➔ чистые `@L[Key]`.
   - Тяжелые блоки `@code` (> 5 строк) ➔ вынос в `*.razor.cs` code-behind.

---

## 2. Пошаговый рефакторинг компонента (Refactor)

### 2.1 Разметка (`*.razor`)
1. **Директивы `@using static` в начале файла**:
   ```razor
   @using static Classes.<Group>
   @using static Icons
   @using static Loc.<Domain>
   @using static Assets.<Group>
   @inject IStringLocalizer L
   ```
2. **Чистый синтаксис атрибутов без кавычек (Unquoted Razor Syntax)**:
   - `Icon=@Close` (вместо `Icon="@Close"`)
   - `OnClick=@CloseDialogAsync` (вместо `OnClick="CloseDialogAsync"`)
   - `Open=@IsOpen` (вместо `Open="@IsOpen"`)
   - `class=@HeaderBar` (вместо `class="@HeaderBar"`)
   - `Variant=@Filled` / `Variant=@Outlined` (вместо `Variant="Filled"` или `Variant=@ButtonVariant.Filled`) через `@using static ButtonVariant`
   - `Type=@Filter` (вместо `Type=@ChipType.Filter`) через `@using static ChipType`
   - `Selected=@(durationMonths == 1)`
   - `Disabled=@(!CanSubmit)`
   - `LeadingIcon=@Payment`
   - `Label=@L[LabelName]`
   - `Value=@Val`

3. **☢️ ТОТАЛЬНЫЙ ЗАКОН: ультра-короткий синтаксис `что-то=@*` — категорический запрет длинных путей с точками `что-то=@*.*.*`**:
   - В разметке `*.razor` **НЕ ДОЛЖНО БЫТЬ** длинных составных выражений с точками вида `что-то=@*.*.*`. Все атрибуты — строго ультра-коротко: `что-то=@*` (или `*=@*`).
   - ❌ **Категорически запрещено**: `class=@Classes.Common.ItemUnitPrice`, `class=@Classes.Orders.HeaderTitleBox`, `Icon=@Icons.Close`, `Variant=@ButtonVariant.Outlined`, `Label=@Loc.Checkout.StepCart`.
   - ✅ **Обязательно**: `class=@ItemUnitPrice`, `class=@HeaderTitleBox`, `Icon=@Close`, `Variant=@Outlined`, `Label=@L[StepCart]`.
   - ❌ **Категорически запрещено создавать алиасы констант в code-behind** (`public const string Alias = global::Assets.***`) ради сокращения имён.
   - ✅ **Исключительно через `using static`**: короткие имена достигаются директивами `@using static Classes.<Group>`, `@using static Icons`, `@using static Loc.<Domain>`, `@using static <EnumName>` в `.razor`; и `using static` в `.razor.cs`.
   - **Разрешение коллизий без точек в разметке**: если две статические группы содержат одноимённые константы — импортировать профильную группу для данного компонента, либо изолировать импорт в заголовке файла. В теле разметки атрибут **всегда** остаётся ультра-коротким: `class=@HeaderTitleBox`.

4. **Особые случаи парсера Razor**:
   - Составные классы без кавычек: вместо интерполяции `class=@($"{Card} {Elevated}")` (содержащей кавычки) строго используется `class=@string.Format(Two, Card, Elevated)` через `@using static Formats` или `class=@string.Format(Temp, ...)` с константой шаблона `const string Temp = "{0} {1} {2}";`. В разметке достигается 100% отсутствие кавычек!
5. **Компоненты `Material.Web` First (Обязательная замена HTML-тегов)**:
   - Если для семантического тега есть Blazor-компонент — **ОБЯЗАТЕЛЬНО использовать его**:
     - Строго `<Button>` (или `<IconButton>`) вместо `<button>`
     - Строго `<TextField>` вместо `<input type="text/email/password">` / `<textarea>`
     - Строго `<Select>` + `<SelectOption>` вместо `<select>` + `<option>`
     - Строго `<Divider>` вместо `<hr>`
     - Строго `<Checkbox>` вместо `<input type="checkbox">`
     - Строго `<Radio>` вместо `<input type="radio">`
     - Строго `<MaterialDialog>` или `<Dialog>` вместо `<dialog>`
     - Строго `<Progress>` вместо `<progress>`
     - Строго `<Tabs>` + `<Tab>` вместо кнопок переключения табов
     - Строго `<Chip>` + `<ChipSet>` вместо самодельных чипов
     - Строго `<Badge>` вместо значков
     - `<Card>` вместо `<div class="card">`
6. **Правильные семантические HTML5 теги (Строго если в `Material.Web` нет готового аналога)**:
   - `<header>`: шапка страницы, карточки или диалога (запрещен `<div class="header">`).
   - `<footer>`: подвал, панель кнопок действий, подтверждение (запрещен `<div class="actions">`).
   - `<main>`: основная рабочая область.
   - `<section>`: логический раздел формы или этап визарда.
   - `<article>`: карточка сущности (товар, бронь, лот, коммент, тарифный план).
   - `<aside>`: сайдбар корзины, фильтры каталога, панель итогов.
   - `<nav>`: навигация, переключатели табов, шаги оформления (`stepper`).
   - `<fieldset>` + `<legend>`: логически связанные группы полей ввода.
   - `<ul>` / `<ol>` + `<li>`: семантические списки опций, фич, позиций заказа.
   - `<time datetime="...">`: даты, временные слоты, таймеры.
7. **Плоское семантическое дерево (Zero Wrapper Bloat)**:
   - Схлопывать лишние промежуточные обертки (`div > div > div`); разметка должна быть максимально плоской.
   - Запрещены `<div>` исключительно ради отступов (`padding`/`margin`) — раскладкой и отступами управляет CSS Grid родительского семантического элемента.
   - Запрещены кликабельные `<div>`, `<span>` или тег `<button>` (строго `<Button>` или `<IconButton>`).

### 2.2 Интерактив и логика (`*.razor.cs`)
1. `public partial class ComponentName : ComponentBase, IAsyncDisposable`
2. Все параметры (`[Parameter]`, `EventCallback`), внедрение зависимостей (`[Inject]`), состояние (`private bool _isOpen`) и хуки жизненного цикла (`OnInitializedAsync`, `DisposeAsync`) выносятся строго в code-behind.
3. **Именование методов-обработчиков (Zero Collisions)**:
   - Именовать с суффиксом `Async` и глаголом (`CloseDialogAsync`, `SubmitOrderAsync`), чтобы исключить коллизии с одноименными иконками `Icons.Close`.
4. **Запрет на создание алиасов констант (`public const string Alias = global::Assets.***`)**:
   - Категорически запрещено плодить в code-behind дублирующие поля-алиасы для классов, иконок, локализации или вариантов (`public const string TitleLarge = global::Assets.Constants.Classes.Typography.TitleLarge;`).
   - Все константы подключаются напрямую через `@using static` в `*.razor` или обычный `using static` в `*.razor.cs`.

### 2.3 3-сторонняя синхронизация локализации (i18n)
Если добавлены новые ключи интерфейса:
1. Добавить ключ симметрично во все 3 файла в `Assets/Resources/`:
   - `ResourceRu.resx` — Русский
   - `ResourceUz.resx` — Узбекский (латиница)
   - `ResourceEn.resx` — Английский
   *(Все символы `&` экранируются как `&amp;`)*.
2. Запустить генератор ключей:
   ```powershell
   npm --prefix src/Frontend/Platform/Shared/DesignSystem/Assets run generate:loc
   ```

---

## 3. Машинная верификация (Machine Gates)

Все гейты обязаны пройти успешно:
```powershell
# 1. Генерация типизированных ключей локализации и классов
npm --prefix src/Frontend/Platform/Shared/DesignSystem/Assets run generate:loc
npm --prefix src/Frontend/Platform/Shared/DesignSystem/Assets run generate:classes

# 2. Чистая сборка проекта
dotnet build <affected csproj> -c Debug
```

**Критерии приемки (DoD)**:
- `dotnet build`: 0 ошибок, 0 новых предупреждений.
- `Flat Semantic Tree`: использованы правильные HTML5 теги (`<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<aside>`, `<nav>`), 0 паразитных `<div>`-оберток ради отступов, 0 кликабельных `div`/`span`.
- `0 Raw HTML Tags with Blazor Equivalents`: 0 `<button>` (строго `<Button>` / `<IconButton>`), 0 `<select>`, 0 `<hr>`, 0 `<input>`.
- `0 Hardcoded Strings`: ни одной строковой константы в разметке.
- `0 Quoted C# Attributes`: ни одного `Icon="@Close"` или `OnClick="Close"`.
- `0 Long Dot Expressions in Attributes`: ни одного `что-то=@*.*.*` (строго короткий синтаксис `что-то=@*` через `@using static`, 0 алиасов констант в code-behind).
- `Ultra-Short Enums`: `@using static <Enum>` ➔ `Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter` (0 длинных `@ButtonVariant.*`).
- `0 Magic Strings`: ни одного `class="..."` или `L["..."]`.
- `0 Prefixes`: ни одного `@Classes.*` или `@L[Loc.*]`.
- `0 Fallbacks`: ни одного `?? "..."`.
- `3-Way Parity`: ключи есть в RU, UZ, EN словарях.

---

## 4. Завершение рефакторинга компонента

После успешного прохождения проверок зафиксировать компонент в истории изменений.
Коммит и пуш выполняются только по прямому указанию пользователя.


