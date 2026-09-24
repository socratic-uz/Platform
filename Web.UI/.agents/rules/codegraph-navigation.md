# CodeGraph Semantic Navigation & Code Intelligence Standards

## 1. 🎯 Purpose & Overview
В экосистеме **Socratic** (150+ проектов C#/.NET 10, Blazor, gRPC, ScyllaDB, субмодули) для навигации, анализа архитектуры и поиска зависимостей используется **CodeGraph** — локальный движок семантического анализа на базе Rust AST Kernel и SQLite FTS5.

CodeGraph устраняет медленный и затратный по токенам перебор файлов («grep & read churn»), предоставляя агентам точный срез исходного кода, цепочки вызовов и радиус влияния (blast radius) за **1 обращение к инструменту**.

---

## 2. ⚡ Приоритет инструментов при исследовании кода

### ✅ Всегда используй CodeGraph (`codegraph_*` / CLI) для:
1. **Архитектурных и смысловых вопросов**:
   - «Как устроен флоу заказа от Checkout до Paying?»
   - «Где реализуется интерфейс `IFaceBiometricProcessor`?»
   - `codegraph_explore` или `codegraph explore "<question>"`
2. **Поиска символов и определений**:
   - Поиск классов, интерфейсов, методов, рекордов: `codegraph_search` / `codegraph_node`
3. **Трассировки вызовов (Call Hierarchy)**:
   - Кто вызывает метод: `codegraph_callers <symbol>`
   - Кого вызывает метод: `codegraph_callees <symbol>`
4. **Анализа влияния изменений (Blast Radius & Impact Analysis)**:
   - Что сломается или изменится при модификации контракта или метода: `codegraph_impact <symbol>`
5. **Определения затрагиваемых тестов**:
   - `codegraph affected [files...]`

### 🔍 Используй `grep_search` ТОЛЬКО для:
- Поиска строковых литералов, конфигурационных ключей в `appsettings.json`, `launchSettings.json`.
- Поиска в XML/CSPROJ свойствах (`<Configurations>`, `<TargetFramework>`).
- Поиска в локализационных `.resx` или markdown-документации.

> **Строгий запрет**: Запрещено запускать каскадные циклы `grep_search` $\rightarrow$ `view_file` $\rightarrow$ `grep_search` для поиска связей C#-классов и интерфейсов, если доступен семантический граф CodeGraph.

---

## 3. 🛠️ Доступные MCP Инструменты CodeGraph

| Инструмент | Назначение | Пример использования |
|---|---|---|
| `codegraph_explore` | **Основной инструмент.** Возвращает верифицированный код релевантных символов, цепочки вызовов и blast-radius | `codegraph_explore(query="How does UniversalProductExperienceRenderer render 28 modes?")` |
| `codegraph_node` | Исходный код конкретного символа с его входящими/исходящими связями | `codegraph_node(name="ProductUxMode")` |
| `codegraph_search` | Быстрый поиск символов по имени через FTS5 | `codegraph_search(query="IFaceBiometricProcessor")` |
| `codegraph_callers` | Все методы/функции, вызывающие указанный символ | `codegraph_callers(symbol="CreateOrderAsync")` |
| `codegraph_callees` | Все методы/функции, которые вызывает указанный символ | `codegraph_callees(symbol="ProcessPaymentAsync")` |
| `codegraph_impact` | Оценка полного радиуса влияния изменения символа | `codegraph_impact(symbol="OrderSaga")` |
| `codegraph_files` | Структура проиндексированных файлов проекта | `codegraph_files()` |
| `codegraph_status` | Состояние и статистика индекса (количество символов, связей) | `codegraph_status()` |

---

## 4. 🔄 Автоматическая синхронизация (Zero-Stale Index)
- CodeGraph непрерывно отслеживает изменения в файловой системе через нативный наблюдатель Windows (`ReadDirectoryChangesW`).
- Любое сохранение или правка файла обновляет граф за 300–400 мс в фоновом режиме.
- Ручной запуск `codegraph sync` не требуется при обычной разработке. При пакетных операциях Git или переключении веток можно вызвать `codegraph sync`.

---

## 5. 🛡️ Git и версионирование
- Каталог `.codegraph/` содержит локальную SQLite базу данных, журналы WAL и lock-файлы.
- Каталог `.codegraph/` **строго изолирован в `.gitignore`** и никогда не коммитится в Git.
