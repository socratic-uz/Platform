# Socratic Messaging & Caching Standards: Apache Kafka & Redis

## 1. Apache Kafka: Event-Driven Streaming Standards

### 1.1 Топология топиков и партиционирование
- **Именование топиков**:
  - `socratic.<domain>.<entity>.<event-type>` (например: `socratic.ordering.orders.created`, `socratic.paying.payments.settled`).
- **Ключ партиционирования (Partition Key)**:
  - **Всегда** использовать идентификатор агрегата (`organization_id` или `order_id`) в качестве ключа партиционирования.
  - Это гарантирует строгую очередность (total order) обработки событий по конкретной организации или заказу внутри одного партишена.

### 1.2 Идемпотентность потребителей (Idempotent Consumers)
- Сеть ненадежна, поэтому Kafka гарантирует доставку *at-least-once*.
- Каждый consumer обязан быть идемпотентным:
  1. При получении события с `event_id` проверить факт обработки в Redis (`SET socratic:events:processed:{event_id} 1 NX EX 86400`) или ScyllaDB.
  2. Если ключ уже существует — залогировать предупреждение и немедленно подтвердить смещение (`Commit`), избегая повторного выполнения бизнес-логики.

### 1.3 Отказоустойчивость: Retry Policies & Dead Letter Queue (DLQ)
- При временных сбоях (сеть, недоступность внешнего PSP):
  - Повторять попытку с экспоненциальной задержкой (Backoff) до 3–5 раз через топик `<topic>-retry`.
- При фатальных ошибках (десериализация, бизнес-инвариант нарушен):
  - Отправлять поврежденное сообщение в `<topic>-dlq` с заголовками ошибки (`x-error-message`, `x-failed-at`, `x-original-topic`).
  - Не блокировать обработку основного партишена (запрет бесконечных циклов краха сервиса).

---

## 2. Redis: Высокопроизводительное кеширование и блокировки

### 2.1 Пространство имен ключей (Key Naming Convention)
Все ключи должны быть структурированы и изолированы по организациям (мультиарендность):
- **Кеш данных**: `socratic:cache:{organization_id}:{entity}:{id}` (TTL обязателен, например 15 минут).
- **Распределенные блокировки**: `socratic:lock:{organization_id}:{resource_name}:{id}` (TTL 5–10 секунд).
- **Сессии и токены**: `socratic:session:{user_id}:{device_id}` (TTL соответствует сроку жизни JWT/Refresh токена).
- **Ограничение частоты (Rate Limiting)**: `socratic:ratelimit:{ip_or_org}:{endpoint}:{window}`.

### 2.2 Распределенные блокировки (Distributed Locks)
Для критических операций (списание баланса, фискализация чека, захват места в кинозале):
```csharp
// Захват блокировки
bool acquired = await redis.StringSetAsync(
    lockKey, 
    lockValue, 
    TimeSpan.FromSeconds(5), 
    When.NotExists);

if (!acquired)
{
    throw new ConcurrencyException("Resource is currently locked by another operation");
}

try
{
    // Критическая секция
}
finally
{
    // Безопасное атомарное освобождение только своей блокировки через Lua-скрипт
    const string luaScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";
    await redis.ScriptEvaluateAsync(luaScript, [lockKey], [lockValue]);
}
```

### 2.3 Предотвращение Cache Stampede
- Запрещено использовать жесткий несинхронизированный сброс кеша на высоконагруженных страницах (каталог популярного ресторана).
- Использовать двухслойный кеш (L1 in-memory `MemoryCache` + L2 `Redis`) с фоновым обновлением при приближении к истечению срока жизни (early probabilistic expiration).
