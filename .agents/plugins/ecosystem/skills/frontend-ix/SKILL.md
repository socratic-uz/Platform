---
name: frontend-ix
description: "Architectural standard and workflow for Blazor C# interactive code-behind (*.razor.cs) and Clean + Vertical Slice architecture in Socratic. USE FOR: pure C# code-behind authoring, partial class component structure, layer classification (Pages/Widgets/Components), UseCase isolation in Features/, separated typed constants, Dependency Injection, component parameters, lifecycle hooks, Global Auto Rendering state persistence, and async resource disposal. DO NOT USE FOR: Razor template markup (use frontend-ux) or CSS styling (use frontend-ui)."
license: MIT
---

# Frontend Skill: C# Interactive Code-Behind & Architecture (*.razor.cs)

Краткое руководство по разработке интерактивной логики Blazor-компонентов и архитектуре фронтенд-модулей.
Полная архитектурная спецификация: [rules/frontend-ix.md](../../../../rules/frontend-ix.md).
Цикл дата-дривен рефакторинга: [skills/ix-refactor-loop/SKILL.md](../ix-refactor-loop/SKILL.md).

---

## 1. Архитектура фронтенд-модулей (Clean + Vertical Slice)

Каждый функциональный модуль организуется по 4-слойной структуре:
- **`Features/`**: чистая C# бизнес-логика (UseCases, команды, DTO). Без Blazor-зависимостей, 100% Native AOT, вызовы через `ValueTask<Result<T>>`.
- **`Pages/`**: каркас экрана и роутинг (`@page "/pos"`), привязка к макро-сетке.
- **`Widgets/`**: умные отраслевые составные организмы (Smart Components: Корзина, Оплата). Внедряют UseCases из `Features/`.
- **`Components/`**: презентационные примитивы (Dumb Components). Общаются с миром только через `[Parameter]` и `EventCallback`, ничего не знают о `Features/` и UseCases.
- **Общий `Domain` & `Infrastructure`**: межмодульные контракты (Value Objects, события, интерфейсы) и порты (gRPC, устройства).

---

## 2. Каноническая структура Code-Behind

Вся C#-логика выносится в парный файл `*.razor.cs`:

```csharp
namespace Socratic.Checkout.Widgets;

public partial class UniversalCheckoutPanel : ComponentBase, IAsyncDisposable
{
    // 1. Уникальные технические константы компонента (DOM ID, ключи хранилища)
    private const string ModalContainerId = "checkout-dialog-container";

    // 2. Внедрение зависимостей (UseCases и сервисы)
    [Inject] private ProcessPaymentUseCase PaymentUseCase { get; set; } = default!;
    [Inject] private PersistentComponentState ApplicationState { get; set; } = default!;

    // 3. Параметры компонента
    [Parameter, EditorRequired] public Guid OrderId { get; set; }
    [Parameter] public EventCallback<PaymentResult> OnPaymentCompleted { get; set; }

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
    private async Task ProcessPaymentAsync()
    {
        if (_isSubmitting) return;
        _isSubmitting = true;

        try
        {
            var command = new ProcessPaymentCommand(OrderId, 100m, PaymentMethod.Card);
            var result = await PaymentUseCase.ExecuteAsync(command);
            if (result.IsSuccess)
            {
                await OnPaymentCompleted.InvokeAsync(result.Value);
            }
        }
        finally
        {
            _isSubmitting = false;
        }
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

## 3. Стандарты работы с константами (Zero Aliases)

- **Константы дизайн-системы**: классы, иконки, локализация и перечисления подключаются напрямую через `@using static` в `*.razor` или обычный `using static` в заголовке `*.razor.cs`. Поля-алиасы (`public const string Alias = ...`) в коде компонента категорически запрещены.
- **Технические константы**: строковые ID элементов, ключи хранилищ и служебные параметры выносятся в `private const string` внутри `*.razor.cs`.

---

## 4. Чеклист проверки (DoD)

- [ ] Бизнес-сценарии вынесены в чистые UseCases в `Features/<Feature>/Application/` (`ValueTask<Result<T>>`).
- [ ] UI разделён на `Pages/` (каркас/роутинг), `Widgets/` (инжектируют UseCases) и `Components/` (чистые примитивы).
- [ ] Примитивы в `Components/` слепы к `Features/` (только `[Parameter]` и `EventCallback`).
- [ ] В `*.razor.cs` нет полей-алиасов констант дизайн-системы (только `using static`).
- [ ] Методы-обработчики именуются с суффиксом `Async` и глаголом (`CloseDialogAsync`, `SubmitOrderAsync`).
- [ ] Состояние кэшируется через `PersistentComponentState` для исключения дублирования запросов в `InteractiveAuto`.
- [ ] Все асинхронные подписки, таймеры и ресурсы освобождаются в `DisposeAsync` / `Dispose`.
- [ ] Вызовы `StateHasChanged()` из внешних потоков обёрнуты в `await InvokeAsync(StateHasChanged)`.


## Universal DOM Lifecycle Dispatcher
All JavaScript modules must register via `registerComponent(selector, { mount, unmount })` in `scripts.js`.
