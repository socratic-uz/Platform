---
name: ix-refactor-loop
description: >-
  Data-driven Blazor C# Code-Behind (*.razor.cs) and architecture refactoring loop, enforcing Clean Architecture + Vertical Slice (Features/UseCases), layer classification (Pages/Widgets/Components), Zero Aliases via using static, PersistentComponentState hydration, IAsyncDisposable lifecycle, and 100% Native AOT compliance.
---

# Workflow: Blazor C# Interaction & Architecture Refactoring Loop (IX)

Цель данного цикла — трансформация кодовой базы фронтенда из монолитных компонентов («God Components») в строгую модульную архитектуру **Clean Architecture + Vertical Slice**, зеркально синхронизированную со слоями стилей Cascade Layers.

Полный архитектурный регламент: [rules/frontend-ix.md](../../../../rules/frontend-ix.md).

---

## 0. Обнаружение целей (Audit & Discovery)

Ищи кандидатов на рефакторинг среди компонентов в `src/Frontend/` (`Platform/`, `Retail/`, `Portal/`, `Intelligence/`, `Studio/`):

1. **"God Components" с раздутой логикой**:
   - Файлы `*.razor.cs` объёмом $> 100$ строк.
   - Компоненты, напрямую внедряющие сетевые gRPC-клиенты, репозитории или выполняющие тяжелые финансовые/бизнес-расчеты.
   - Компоненты, содержащие приватные методы бизнес-валидации, которые должны тестироваться изолированно.
2. **Нарушения слоёв и связности**:
   - Атомарные компоненты в `Components/`, которые инжектируют бизнес-сервисы вместо общения через `[Parameter]` и `EventCallback`.
   - Виджеты или экраны, перемешанные в одной папке без разделения на `Pages/` и `Widgets/`.
3. **Нарушения Zero Aliases**:
   - Наличие полей-алиасов дизайн-системы вида `public const string Alias = global::Assets.***` или `public string PropClass => Classes.***`.
4. **Проблемы жизненного цикла и гидратации**:
   - Отсутствие `PersistentComponentState` в компонентах экранов с `InteractiveAuto` (приводит к двойным сетевым запросам и морганию UI).
   - Подписки на аппаратные службы (сканер, PinPad, фискальный регистратор) или таймеры без реализации `IAsyncDisposable` / `IDisposable`.
   - Вызовы `StateHasChanged()` из событий оборудования без обертки в `await InvokeAsync(StateHasChanged)`.

---

## 1. Пошаговый алгоритм рефакторинга

```
[Исходный God Component] 
           │
           ├─ 1. Бизнес-сценарий  ──> Features/<Feature>/<UseCase>.cs
           ├─ 2. DTO и Команды    ──> Features/<Feature>/<Feature>Contracts.cs
           ├─ 3. Роутинг / Сетка  ──> Pages/<Screen>Page.razor (.cs)
           ├─ 4. Умный организм   ──> Widgets/<Domain>Widget.razor (.cs)
           ├─ 5. UI-примитивы     ──> Components/<Atomic>Component.razor (.cs)
           └─ 6. Общие контракты  ──> Shared Domain / Infrastructure
```

### Шаг 1. Извлечение бизнес-логики в UseCase (`Features/<Feature>/`)
1. Создай чистый C# POCO-класс сценария:
   ```csharp
   namespace Socratic.PointOfSale.Features.Checkout.Application;

   public sealed record ProcessPaymentCommand(Guid OrderId, decimal Amount, PaymentMethod Method);

   public sealed class ProcessPaymentUseCase
   {
       private readonly IPaymentGateway _gateway;
       private readonly IPosCartStore _cartStore;

       public ProcessPaymentUseCase(IPaymentGateway gateway, IPosCartStore cartStore)
       {
           _gateway = gateway;
           _cartStore = cartStore;
       }

       public async ValueTask<Result<PaymentReceiptDto>> ExecuteAsync(
           ProcessPaymentCommand command, 
           CancellationToken ct = default)
       {
           if (command.Amount <= 0)
               return Result.Failure<PaymentReceiptDto>("Сумма должна быть больше 0");

           var result = await _gateway.PayAsync(command, ct);
           if (!result.IsSuccess)
               return Result.Failure<PaymentReceiptDto>(result.Error);

           _cartStore.MarkPaid(result.Value.TransactionId);
           return Result.Success(result.Value);
       }
   }
   ```
2. **Требования к UseCase**:
   - Полный запрет зависимостей от Blazor (`ComponentBase`, `NavigationManager`, `RenderTree`).
   - Возврат только через `ValueTask<Result<T>>` (Zero-allocation при синхронном успехе/кэше).
   - Никаких `throw new Exception` для бизнес-ошибок (только `Result.Failure`).
   - 100% Native AOT: явная регистрация в DI `services.AddScoped<ProcessPaymentUseCase>()`.

### Шаг 2. Классификация и размещение UI по 3 слоям
1. **`Pages/` (Каркас экрана)**:
   - Содержит атрибуты маршрутизации `[Route("/path")]` и авторизации `[Authorize]` над `partial class` в `*.razor.cs` (директивы `@page` и `@attribute` в `*.razor` строго запрещены).
   - Задает разметку сетки CSS Grid (макро-лейаут).
   - Читает URL query-параметры.
2. **`Widgets/` (Умный организм / Smart Component)**:
   - Составная бизнес-панель (Корзина, Оплата, Фискальный блок).
   - **Инжектирует UseCase**: `[Inject] private ProcessPaymentUseCase PaymentUseCase { get; set; } = default!;`
   - Вызывает `await PaymentUseCase.ExecuteAsync(command, ct)`.
3. **`Components/` (Презентационный примитив / Dumb Component)**:
   - **Строго слеп к `Features/`**: никаких UseCases в инжекциях!
   - Входные данные — только через `[Parameter]`.
   - События наружу — только через `EventCallback` / `EventCallback<T>`.

### Шаг 3. Искоренение констант-алиасов (Zero Aliases)
- Удали из `*.razor.cs` любые поля и свойства-алиасы:
  ```csharp
  // ❌ УДАЛИТЬ:
  public const string TitleLarge = Classes.Typography.TitleLarge;
  public string CardClass => Classes.Cards.CardElevated;
  ```
- Подключи группы напрямую через `using static` в `*.razor.cs` или `@using static` в `*.razor`:
  ```csharp
  using static Assets.Constants.Classes.Cards;
  using static Assets.Constants.Classes.Buttons;
  ```
- Оставь в классе только уникальные технические константы:
  ```csharp
  private const string DialogDomId = "checkout-modal-root";
  ```

### Шаг 4. Гидратация Global Auto Rendering (`PersistentComponentState`)
Для экранов и ключевых виджетов исключи повторные gRPC-запросы при переходе SSR ➔ WASM:
```csharp
[Inject] private PersistentComponentState ApplicationState { get; set; } = default!;
private PersistingComponentStateSubscription _persistSubscription;
private OrderDto? _order;

protected override async Task OnInitializedAsync()
{
    _persistSubscription = ApplicationState.RegisterOnPersisting(PersistState);

    if (!ApplicationState.TryTakeFromJson<OrderDto>("order_cache", out var cached))
    {
        var result = await GetOrderUseCase.ExecuteAsync(OrderId);
        _order = result.Value;
    }
    else
    {
        _order = cached;
    }
}

private Task PersistState()
{
    ApplicationState.PersistAsJson("order_cache", _order);
    return Task.CompletedTask;
}
```

### Шаг 5. Освобождение ресурсов и потокобезопасность
1. Реализуй `IAsyncDisposable`:
   ```csharp
   public async ValueTask DisposeAsync()
   {
       _persistSubscription.Dispose();
       if (_scannerService is not null)
       {
           _scannerService.OnBarcodeScanned -= HandleBarcodeScanned;
       }
   }
   ```
2. Оберни внешние события в маршалинг потока:
   ```csharp
   private void HandleBarcodeScanned(string barcode)
   {
       _ = InvokeAsync(async () =>
       {
           await AddItemByBarcodeUseCase.ExecuteAsync(barcode);
           StateHasChanged();
       });
   }
   ```

---

## 2. Чеклист проверки (Definition of Done)

- [ ] Вся бизнес-логика вынесена из `*.razor.cs` в UseCase внутри `Features/<Feature>/`.
- [ ] UseCase возвращает `ValueTask<Result<T>>` и не содержит зависимостей от Blazor.
- [ ] UI-слой четко разделён: `Pages/` (роутинг), `Widgets/` (инжектируют UseCases), `Components/` (чистые примитивы).
- [ ] Примитивы в `Components/` не знают про UseCases (только `[Parameter]` и `EventCallback`).
- [ ] В `*.razor.cs` отсутствуют свойства и поля-алиасы констант дизайн-системы (только `using static`).
- [ ] Все обработчики событий названы с глаголом и суффиксом `Async` (`CloseDialogAsync`, `SubmitOrderAsync`).
- [ ] Для экранов реализован `PersistentComponentState` (0 двойных вызовов в `InteractiveAuto`).
- [ ] Все аппаратные подписки, таймеры и стримы отписываются в `DisposeAsync` / `Dispose`.
- [ ] Все вызовы `StateHasChanged()` из аппаратных потоков обёрнуты в `await InvokeAsync(...)`.
- [ ] Сборка компилируется без ошибок и предупреждений: `dotnet build <module>.csproj -c Debug`.
- [ ] Написан Unit-тест на UseCase без поднятия рендерера Blazor.

