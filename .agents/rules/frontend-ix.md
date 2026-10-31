# Архитектура фронтенда: 2. Интерактив и логика в *.razor.cs файлах

## 1. 🏛️ Золотой стандарт: Чистый C# Code-Behind

Все поведение пользовательского интерфейса, внедрение зависимостей, управление состоянием и обработка событий реализуются **строго в парных файлах `*.razor.cs` (code-behind)**.

> **ГЛАВНЫЙ ЗАКОН ИНТЕРАКТИВА**:
> Файлы `*.razor` содержат только разметку, а файлы `*.razor.cs` содержат чистый C#-код (`public partial class ComponentName : ComponentBase`). Никаких тяжелых блоков `@code` внутри `.razor` разметки.

---

## 2. 🧱 Структура файла Code-Behind (*.razor.cs)

Каждый компонент со сложной логикой, параметрами или состоянием разделяется на:
- `MyComponent.razor` — шаблон разметки;
- `MyComponent.razor.cs` — класс частичной реализации (code-behind).

```csharp
namespace Socratic.Checkout.Widgets;

public partial class UniversalCheckoutPanel : ComponentBase, IAsyncDisposable
{
    // 1. Технические константы компонента (DOM IDs, technical keys)
    // 2. Внедрение зависимостей ([Inject])
    // 3. Параметры компонента ([Parameter])
    // 4. Локальное состояние и поля
    // 5. Хуки жизненного цикла (OnInit, OnParams, Dispose)
    // 6. Обработчики событий и бизнес-методы
}
```

---

## 3. 🎯 Статические импорты через `using static` вместо создания алиасов в code-behind

В проектах Socratic **КАТЕГОРИЧЕСКИ ЗАПРЕЩЕНО создавать прокси- или алиас-константы и свойства-геттеры в `*.razor.cs`** вида:
```csharp
// ❌ СТРОГО ЗАПРЕЩЕНО: создание констант-алиасов или свойств-геттеров в code-behind
public const string TitleLarge = global::Assets.Constants.Classes.Typography.TitleLarge;
public const string OrderItemInfo = global::Assets.Constants.Classes.InfoGrid.OrderItemInfo;
public const string StepCartLoc = global::Assets.Constants.Loc.Checkout.StepCart;
public const string CloseIcon = global::Assets.Constants.Icons.Close;
public const ButtonVariant Filled = ButtonVariant.Filled;
public string ItemCardClass => Classes.Cards.ItemCard; // ❌ ЗАПРЕЩЕНО: свойства-алиасы классов
public string OutlinedCard => Classes.Cards.CardOutlined; // ❌ ЗАПРЕЩЕНО: свойства-алиасы классов
```

Вместо дублирования и засорения классов компонентов алиасами, **ОБЯЗАТЕЛЬНО использовать директивы `using static`**:
- В `*.razor` разметке напрямую подключаются статические группы:
  - `@using static Classes.<Group>`
  - `@using static Icons`
  - `@using static Loc.<Domain>`
  - `@using static Assets.<Group>`
  - `@using static <EnumName>`

```razor
@using static Classes.Cards
@using static Classes.Buttons
@using static Classes.Typography
@using static Icons
@using static Loc.Checkout
@using static ButtonVariant
```

- Если константа классов или локализации требуется в логике C# (`*.razor.cs`), она импортируется через обычный `using static` в заголовке C#-файла:
```csharp
using static Assets.Constants.Classes.Orders;
using static Assets.Constants.Loc.Checkout;
```
но **никогда не объявляется повторно как константное поле компонента** (`public const string ... = ...`).

### Вынос строковых констант из разметки
В `.razor` файлах категорически запрещены строковые литералы вида `"любой текст"`.
1. **Любой пользовательский текст, заголовки, сообщения и метки** выносятся строго в `.resx` словари локализации и вызываются как `@L[Key]`.
2. **Технические строковые константы, параметры, ключи или идентификаторы**, если они не локализуются, выносятся отдельно в `*.razor.cs` как типизированные константы (`private const string ...` или `public const string ...`):

```csharp
namespace Socratic.Checkout.Widgets;

public partial class UniversalCheckoutPanel
{
    private const string ModalContainerId = "checkout-dialog-container";
    private const string SvgViewBox = "0 0 24 24";
}
```

> **ПРАВИЛО**: Разметка `*.razor` никогда не содержит сырых строковых литералов `"любой текст"`. Всё вынесено в локализацию либо в типизированные константы.

---

## 4. ⚡ Режим Global Auto Rendering & Защита от дублирования данных

Socratic Web UI работает в режиме **Global Auto Rendering** (`InteractiveAuto`):
1. **Первый рендер**: серверная генерация (SSR Prerender) для мгновенной отдачи HTML.
2. **Гидратация**: запуск клиентского WebAssembly / SignalR для полной интерактивности.

### Защита от повторной загрузки данных (PersistentComponentState):
Чтобы исключить двойные запросы к gRPC / API и мигание данных при гидратации, состояние обязательно кэшируется через `PersistentComponentState`:

```csharp
[Inject] private PersistentComponentState ApplicationState { get; set; } = default!;
private PersistingComponentStateSubscription _persistSubscription;
private List<ProductDto> _products = new();

protected override async Task OnInitializedAsync()
{
    _persistSubscription = ApplicationState.RegisterOnPersisting(PersistData);

    if (!ApplicationState.TryTakeFromJson<List<ProductDto>>("products_cache", out var cached))
    {
        // Выполняется только при первом SSR-рендере на сервере
        _products = await ProductClient.GetProductsAsync();
    }
    else
    {
        // На клиенте (WASM) мгновенно берем сериализованные данные из SSR
        _products = cached ?? new();
    }
}

private Task PersistData()
{
    ApplicationState.PersistAsJson("products_cache", _products);
    return Task.CompletedTask;
}
```

---

## 5. 🔄 Жизненный цикл и освобождение ресурсов (IAsyncDisposable)

- **Отписка от событий и таймеров**: любые подписки на шину сообщений, аппаратные службы (PinPad, CashAcceptor) или фоновые таймеры обязаны очищаться в `Dispose` / `DisposeAsync`.
- **Потокобезопасность**: при обновлении состояния из фоновых потоков, gRPC-стримов или аппаратных колбэков вызов `StateHasChanged()` обязательно оборачивается в `await InvokeAsync(StateHasChanged)`.

```csharp
public async ValueTask DisposeAsync()
{
    _persistSubscription.Dispose();

    if (_pollingTimer is not null)
    {
        await _pollingTimer.DisposeAsync();
    }
}
```

---

## 6. 📄 Канонический пример чистого файла Code-Behind (*.razor.cs)

```csharp
namespace Socratic.Checkout.Widgets;

public partial class UniversalCheckoutPanel : ComponentBase, IAsyncDisposable
{
    // --- 1. Разрешение коллизий и технические константы ---
    // Большинство констант импортируются в *.razor через @using static.
    // Приватные константы используются только при прямых коллизиях имен или для внутренних идентификаторов:
    private const string ModalContainerId = "checkout-dialog-container";

    // --- 2. Внедрение зависимостей ---
    [Inject] private IPosPinPadService? PosPinPadService { get; set; }
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // --- 3. Параметры компонента ---
    [Parameter, EditorRequired] public Guid OrderId { get; set; }
    [Parameter, EditorRequired] public decimal TotalAmount { get; set; }
    [Parameter] public EventCallback<PaymentResult> OnPaymentCompleted { get; set; }

    // --- 4. Состояние ---
    private bool _isProcessing;
    private int _activeTabIndex;

    // --- 5. Жизненный цикл ---
    protected override async Task OnInitializedAsync()
    {
        if (PosPinPadService is not null)
        {
            PosPinPadService.OnStatusChanged += HandlePinPadStatusChanged;
        }
        await Task.CompletedTask;
    }

    // --- 6. Обработчики событий ---
    private async Task ProcessPaymentAsync()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            // Обработка платежа
            if (OnPaymentCompleted.HasDelegate)
            {
                await OnPaymentCompleted.InvokeAsync(new PaymentResult { Success = true });
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void HandlePinPadStatusChanged(object? sender, string status)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public ValueTask DisposeAsync()
    {
        if (PosPinPadService is not null)
        {
            PosPinPadService.OnStatusChanged -= HandlePinPadStatusChanged;
        }
        return ValueTask.CompletedTask;
    }
}
```

---

## 8. ✅ Definition of Done (DoD) для интерактива (*.razor.cs)

Перед сдачей любого компонента с кодом агент обязан проверить:
1. **0 Build Errors & Warnings**: `dotnet build <affected csproj> -c Debug` проходит с нулевым количеством ошибок и предупреждений компилятора.
2. **Чистый Code-Behind**: Вся логика, внедрение зависимостей (`[Inject]`), параметры (`[Parameter]`, `EventCallback`) и состояние вынесены в `*.razor.cs`.
3. **Zero Aliases — только `using static`**: В `*.razor.cs` категорически запрещены поля-алиасы констант дизайн-системы (`public const string Alias = global::Assets.***`). Все константы классов, иконок, локализации и перечислений подключаются через `using static Classes.<Group>`, `using static Icons`, `using static Loc.<Domain>`, `using static <EnumName>`. Объявляются только уникальные технические константы компонента (DOM ID, storage key и т.п.).
4. **PersistentComponentState**: Для страниц и виджетов в режиме Auto Rendering реализована защита от двойной загрузки данных при гидратации.
5. **Безопасное освобождение (Dispose/DisposeAsync)**: Все подписки на события сервисов, таймеры и стримы корректно отписываются в `IDisposable` или `IAsyncDisposable`.
6. **Потокобезопасность UI**: Вызовы `StateHasChanged()` из фоновых потоков или аппаратных событий обернуты в `await InvokeAsync(StateHasChanged)`.
7. **Zero Reflection & Native AOT**: Отсутствуют вызовы `dynamic`, reflection, runtime emit; все DTO сериализуются через source-generated JSON-контексты.
