# Socratic Architectural Constraints: Backend, Storage, Messaging & APIs

## 1. Архитектурный стиль Backend: Clean + DDD + Vertical Slice
- **Clean Architecture + Domain-Driven Design (DDD)**:
  - **Domain**: Сущности (`Entities`), объекты-значения (`Value Objects`), доменные события (`Domain Events`), бизнес-правила. Нулевые внешние зависимости.
  - **Application**: Команды (`Commands`), запросы (`Queries`), обработчики (`Handlers`), DTO, интерфейсы репозиториев и внешних сервисов.
  - **Infrastructure**: Реализация репозиториев ScyllaDB, клиенты Kafka, кеширование Redis, хранилище MinIO, интеграции.
  - **Presentation**: gRPC сервисы, фоновые потребители событий (consumers), внешние webhooks.
- **CQRS**: При высоких нагрузках разделять потоки чтения (Query) и записи (Command). Не использовать одинаковые модели для чтения и записи.

---

## 2. База данных: ScyllaDB NoSQL (Строгий запрет EF Core)
- **Основная БД**: **ScyllaDB** (C++ NoSQL, Cassandra-совместимая) через C# драйвер `Cassandra.ISession`.
- **Запрет EF Core**: Не использовать Entity Framework Core. EF Core активно использует reflection, expression trees, runtime metadata и change tracking, что ухудшает startup time, latency и ломает Native AOT.
- **Query-Driven Design**:
  - Каждая таблица проектируется под конкретный сценарий чтения.
  - **Мультиарендность**: обязательное присутствие `organization_id` в partition key (`WHERE organization_id = ? AND id = ?`).
  - Обязательный учет clustering keys, размера partition (не более 100MB на партицию), write amplification, consistency level и eventual consistency.
  - Все запросы должны использовать `PreparedStatement` с кешированием на время жизни сервиса.
  - Никаких SQL relational joins — только денормализация или параллельные точечные асинхронные запросы.

---

## 3. События и Messaging: Apache Kafka
- Kafka используется для:
  - Доменных и интеграционных событий между микросервисами.
  - Event streaming и асинхронной обработки задач.
- **Обязательные требования**:
  - **Idempotent Consumers**: каждый consumer обязан безопасно обрабатывать дубли сообщений.
  - **Retry Policies & Dead Letter Queues (DLQ)**: гарантированная изоляция ядовитых сообщений.
  - **Partitioning Strategy**: выбор ключа партиционирования для сохранения порядка событий по организации/заказу.
  - **Schema Evolution**: обратная совместимость контрактов сообщений.
  - **Запрет синхронных вызовов**: не делать синхронные вызовы между микросервисами без абсолютной необходимости.

---

## 4. Кеширование и распределенные блокировки: Redis
- Redis используется для:
  - Горячих данных (`hot data`) и распределенного кеша.
  - Сессий пользователей и распределенного rate limiting.
  - Распределенных блокировок (`distributed locks`) для предотвращения double-spend и race conditions.
- **Всегда анализировать**:
  - Cache invalidation стратегии.
  - Time-To-Live (TTL) для каждого ключа.
  - Нагрузку на память (memory pressure).
  - Степень консистентности кеша.

---

## 5. Объектное хранилище: MinIO (S3 Compatible)
- MinIO используется как S3 object storage:
  - Изображения товаров, баннеры, видео, аудио, аватары пользователей, документы, логи и бэкапы.
- **Критическое правило**: **Никогда не хранить большие бинарные данные (BLOB > 64KB) в ScyllaDB.**

---

## 6. Dependency Injection (DI)
- Избегать Service Locator (`serviceProvider.GetService<T>()`).
- Избегать массовой регистрации через reflection scanning (`services.Scan(...)`).
- Предпочитать:
  - Явную регистрацию (`explicit registration`).
  - Source-generated DI там, где доступно.
  - Singleton только для stateless объектов или пулов ресурсов.

---

## 7. Сетевое взаимодействие и API
1. **gRPC (Protobuf)**: стандарт по умолчанию для межсервисного взаимодействия и связи Blazor WebApp с бекендом через gRPC-Web (стриминг, сжатие, строгая типизация).
2. **REST / HTTP**: только для внешних платежных шлюзов (webhooks PSP) и интеграций с третьими сторонами.
3. **SignalR / WebSocket / gRPC Streaming**: для realtime обновлений статусов заказов, чеков и кассовых оповещений.

---

## 8. Federated Meta-Repository, Stream-Aligned Teams & Self-Contained Feature Slices (SCS)

### 8.1 Архитектурный принцип автономности команд
- Архитектура фронтенда и микросервисов построена по модели **Stream-Aligned Teams (Team Topologies)** и **Self-Contained Systems (SCS)**.
- Каждая продуктовая фича (`Commerce`, `POS`, `Kiosk`, `Vision`) является полностью **автономным Git-репозиторием** с собственным решением (`.slnx`), тестами, локальным хостом (`Web.UI`) и подмодулями платформы (`Core`, `DesignSystem`, `Hardware`, `SharedKernel`).
- Разработчик фичи клонирует **только свою фичу** (`git clone --recurse-submodules https://github.com/socratic-uz/<Feature>.git`) и работает без необходимости скачивать монорепозиторий или другие домены.

### 8.2 Разделение зависимостей и Decoupled Web.UI Host
- **Категорический запрет прямых связей между фичами**: `Commerce` не может ссылаться на `POS`, `Kiosk` не может ссылаться на `Vision`.
- Общие контракты, DTO и gRPC-прототипы выносятся исключительно в `SharedKernel`.
- Универсальный хост `Web.UI` подключает фичи через компиляционные константы (`FEATURE_COMMERCE`, `FEATURE_POS`, `FEATURE_KIOSK`, `FEATURE_VISION`) или динамическое обнаружение Razor-компонентов. Отсутствие директивы сборки для фичи не должно приводить к ошибкам компиляции хоста.

### 8.3 Стандартизация конфигураций сборки
- Все проекты (.csproj) обязаны поддерживать 5 стандартных конфигураций:
  `<Configurations>Debug;Release;LocalDebug;Runner;Cluster</Configurations>`.
- Сборка и запуск должны работать идентично как в Standalone-режиме фичи, так и в рамках Umbrella Meta-Repository (`Socratic.slnx`, `Frontend.slnx`).

