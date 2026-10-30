---
name: frontend-razor-markup
description: "Architectural standard and workflow for Blazor Razor template markup (*.razor) in Socratic. USE FOR: pure HTML5 template authoring, Material.Web component priority, mandatory using static for ALL constants (Classes, Icons, Loc, Assets, Enums) with ultra-short names, unquoted syntax, 3-way .resx parity, and accessibility. DO NOT USE FOR: C# business logic (use frontend-csharp-interactive) or CSS styling (use frontend-css-styles)."
license: MIT
---

# Frontend Skill: Razor Markup (*.razor)

Краткое руководство по разработке и рефакторингу Razor-разметки в проектах Socratic.
Полная архитектурная спецификация: [rules/frontend-razor-markup.md](../../../../rules/frontend-razor-markup.md).

---

## 1. Подключение констант: `@using static`

Все константы классов, иконок, локализации и перечислений подключаются в начале `.razor` файла через директивы `@using static`:

```razor
@using static Classes.Cards
@using static Classes.Buttons
@using static Classes.Typography
@using static Icons
@using static Loc.Checkout
@using static ButtonVariant
@using static ChipType
@inject IStringLocalizer L
```

В разметке используются строго короткие имена:
- Классы: `class=@Card`, `class=@HeaderTitleBox`, `class=@($"{Card} {Elevated}")`
- Иконки: `Icon=@Close`, `Icon=@Add`, `Icon=@Search`
- Локализация: `@L[HeaderTitle]`, `Label=@L[StepCart]`
- Перечисления: `Variant=@Outlined`, `Variant=@Filled`, `Type=@Filter`

---

## 2. Синтаксис атрибутов: ультра-короткий и без кавычек

Все параметры компонентов, обработчики событий, иконки, классы и перечисления оформляются в синтаксисе без кавычек в ультра-коротком формате `что-то=@*` (или `*=@*`):

```razor
<Button Variant=@Filled LeadingIcon=@Add OnClick=@SaveAsync>@L[Save]</Button>
<IconButton Icon=@Close OnClick=@CloseDialogAsync AriaLabel=@L[CloseAction] />
<TextField Label=@L[Email] Value=@Email />
<Chip Type=@Filter Selected=@IsSelected>@L[FilterActive]</Chip>
```

- Строковые HTML-атрибуты и слоты остаются в кавычках: `Slot="icon"`, `type="button"`.
- Дженерики экранируются скобками: `Variant=@(TextField<decimal>.TextFieldVariant.Outlined)`.
- Исключены любые составные выражения с точками в атрибутах (`что-то=@*.*.*`).
- Исключены поля-алиасы констант в `*.razor.cs` — все короткие имена подключаются через `using static`.

---

## 3. Компоненты Material.Web First & Семантический HTML5

1. **При наличии готового Blazor-компонента в `Material.Web` используется он**:
   - `<Button>` / `<IconButton>` вместо `<button>`
   - `<TextField>` вместо `<input type="text">` / `<textarea>`
   - `<Select>` + `<SelectOption>` вместо `<select>` + `<option>`
   - `<Divider>` вместо `<hr>`
   - `<Checkbox>` вместо `<input type="checkbox">`
   - `<Radio>` вместо `<input type="radio">`
   - `<MaterialDialog>` вместо `<dialog>`
   - `<Progress>` вместо `<progress>`
   - `<Tabs>` + `<Tab>` вместо самодельных табов
   - `<Chip>` + `<ChipSet>` вместо самодельных чипов
2. **Семантические теги HTML5**: `<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, `<aside>`, `<nav>`, `<fieldset>`, `<legend>`, `<ul>`/`<li>`.
3. **Плоская структура (Zero Wrapper Bloat)**: промежуточные `<div>` ради отступов не создаются; раскладка задаётся CSS Grid на родительском семантическом элементе.

---

## 4. Чеклист проверки (DoD)

- [ ] `dotnet build`: компиляция без ошибок и предупреждений.
- [ ] Все атрибуты записаны без кавычек в ультра-коротком формате `что-то=@*`.
- [ ] Все константы и перечисления импортированы через `@using static` (0 составных путей с точками в разметке, 0 алиасов в code-behind).
- [ ] Весь UI-текст вынесен в `.resx` словари (`@L[Key]`), синхронизирован в RU, UZ, EN.
- [ ] Для всех элементов с Blazor-эквивалентом использован компонент `Material.Web`.
- [ ] Нет блоков `<style>`, инлайн-стилей `style="..."` и тяжелых блоков `@code` (логика в `*.razor.cs`).
