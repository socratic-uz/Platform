# Socratic Testing & Fixtures Guidelines

This document establishes standards for unit and integration testing in Socratic backend and frontend test suites.

---

## 1. Test Project Structure
Each microservice has an associated test project:
- `src/Backend/Identifying/Identifying.Test/`
- `src/Backend/Shopping/Shopping.Test/`
- `src/Backend/Ordering/Ordering.Test/`
- `src/Backend/Paying/Paying.Test/`
- `src/Backend/Map/Map.Test/`

### Решения в формате .slnx:
Socratic использует современный формат решений **`.slnx`** (XML-based solution):
- Полное решение: `Socratic.slnx`
- Подсистемы: `Shared.slnx`, `UI.slnx`, `Shopping.slnx`, `Ordering.slnx`, `Paying.slnx`, `Identifying.slnx`
- Команда запуска всех тестов: `dotnet test Socratic.slnx --filter "Category!=Integration"`

---

## 2. Unit Testing: Mocking ScyllaDB ISession
For pure domain and business logic testing, do NOT attempt live connections to ScyllaDB. Mock the driver session:

```csharp
using Moq;
using Cassandra;

public class ScyllaOrderRepositoryTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<PreparedStatement> _prepStmtMock;

    public ScyllaOrderRepositoryTests()
    {
        _sessionMock = new Mock<ISession>();
        _prepStmtMock = new Mock<PreparedStatement>();

        _sessionMock.Setup(s => s.PrepareAsync(It.IsAny<string>()))
            .ReturnsAsync(_prepStmtMock.Object);
    }
}
```

---

## 3. Integration Testing: ScyllaFixture & Testcontainers
- Integration tests requiring live CQL execution use `ScyllaFixture` (e.g. in `Paying.Test/Integration/ScyllaFixture.cs`).
- Integration tests must handle connection retries gracefully if containers are spinning up.
- Never hardcode production IP addresses or production keyspace names in test configurations.
