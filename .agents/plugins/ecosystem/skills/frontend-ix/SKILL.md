---
name: frontend-ix
description: "Architectural standard and workflow for Blazor C# interactive code-behind (*.razor.cs) in Socratic. USE FOR: pure C# code-behind authoring, partial class component structure, separated typed constants, Dependency Injection, component parameters, lifecycle hooks, Global Auto Rendering state persistence, and async resource disposal. DO NOT USE FOR: Razor template markup (use frontend-ux) or CSS styling (use frontend-ui)."
license: MIT
---

# Frontend Skill: C# Interactive Code-Behind (*.razor.cs)

Краткое руководство по разработке интерактивной логики Blazor-компонентов.
Полная архитектурная спецификация: [rules/frontend-ix.md](../../../../rules/frontend-ix.md).

---

## 1. Каноническая структура Code-Behind

Вся C#-логика выносится в парный файл `*.razor.cs`:

```csharp
namespace Socratic.Checkout.Widgets;

public partial class UniversalCheckoutPanel : ComponentBase, IAsyncDisposable
{
    // 1. Уникальные технические константы компонента (DOM ID, ключи хранилища)
    private const string ModalContainerId = "checkout-dialog-container";

    // 2. Внедрение зависимостей
    [Inject] private IStringLocalizer L { get; set; } = default!;
    [Inject] private PersistentComponentState ApplicationState { get; set; } = default!;

    // 3. Параметры компонента
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    // 4. Локальное состояние
    private bool _isSubmitting;
    private PersistingComponentStateSubscription _persistSubscription;

    // 5. Хуки жизненного цикла
    protected override async Task OnInitializedAsync()
    {
        _persistSubscription = ApplicationState.RegisterOnPersisting(PersistData);
        if (!ApplicationState.TryTakeFromJson<OrderData>("order_data", out var restored))
        {
            await LoadDataAsync();
        }
    }

    // 6. Обработчики событий (именование с Async и глаголом)
    private async Task CloseDialogAsync()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    // 7. Освобождение ресурсов
    public async ValueTask DisposeAsync()
    {
        _persistSubscription.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

---

## 2. Стандарты работы с константами (Zero Aliases)

- **Константы дизайн-системы**: классы, иконки, локализация и перечисления подключаются напрямую через `@using static` в `*.razor` или обычный `using static` в заголовке `*.razor.cs`. Поля-алиасы (`public const string Alias = ...`) в коде компонента не создаются.
- **Технические константы**: строковые ID элементов, ключи хранилищ и служебные параметры выносятся в `private const string` внутри `*.razor.cs`.

---

## 3. Чеклист проверки (DoD)

- [ ] Вся бизнес-логика, параметры и хуки вынесены в `*.razor.cs` (в `.razor` нет тяжелых блоков `@code`).
- [ ] В `*.razor.cs` нет полей-алиасов констант дизайн-системы (только `using static`).
- [ ] Методы-обработчики именуются с суффиксом `Async` и глаголом (`CloseDialogAsync`, `SubmitOrderAsync`).
- [ ] Состояние кэшируется через `PersistentComponentState` для исключения дублирования запросов в `InteractiveAuto`.
- [ ] Все асинхронные подписки, таймеры и ресурсы освобождаются в `DisposeAsync` / `Dispose`.
