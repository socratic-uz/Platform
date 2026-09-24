# Native AOT & Trimming Safety Guidelines (.NET 10+)

Socratic проектируется с обязательным учетом **Native AOT** и максимальной готовности к триммингу (.NET 10+).

---

## 1. Фундаментальные требования Native AOT
- **Строгий запрет**:
  - Runtime reflection (`Assembly.GetTypes()`, `GetMethod()`, `Activator.CreateInstance`).
  - Runtime code generation (`Reflection.Emit`, `Expression.Compile()`).
  - `dynamic` и неявные late-bound вызовы.
  - Сканирование сборок для DI (`services.Scan(...)`).
  - Нетипизированная JSON-сериализация без source generation.
- **Ошибки сборщика**:
  - Все предупреждения тримминга и AOT (`IL2026`, `IL3050`, `RequiresUnreferencedCode`, `RequiresDynamicCode`) считать **критическими ошибками проектирования** и немедленно исправлять.
  - В проектах включены флаги: `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`, `<EnableSingleFileAnalyzer>true</EnableSingleFileAnalyzer>`.

---

## 2. System.Text.Json Source Generation
Все операции с JSON обязаны использовать строго типизированный контекст `JsonSerializerContext`:

```csharp
[JsonSourceGenerationOptions(
    WriteIndented = false, 
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSerializable(typeof(OrderDto))]
[JsonSerializable(typeof(PaymentResult))]
public partial class AotJsonContext : JsonSerializerContext { }
```

### Вызов сериализации:
```csharp
// Исключительно через AotJsonContext.Default.*
var product = JsonSerializer.Deserialize(jsonSpan, AotJsonContext.Default.ProductDto);
var json = JsonSerializer.Serialize(product, AotJsonContext.Default.ProductDto);
```

---

## 3. Microsoft.Extensions.Hosting & Explicit Registration
- Использовать AOT-compatible подход для хостинга: `WebApplication.CreateSlimBuilder(args)` или `builder.Services.ConfigureHttpJsonOptions(...)`.
- Все сервисы и зависимости регистрируются явно (`services.AddSingleton<IProductRepository, ScyllaProductRepository>()`), без сканирования сборок в runtime.
- Использовать compile-time реестры (например, `ProductModuleRegistry`) вместо динамического поиска типов по строковому имени.
