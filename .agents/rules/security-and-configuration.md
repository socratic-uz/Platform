# Socratic Security & Configuration Guidelines

This document sets architectural rules for credentials, secrets, network addresses, and configuration management across all Socratic services.

---

## 1. Secrets & Credentials Management

- **Zero Hardcoded Secrets**: Passwords, JWT secrets, private keys, payment provider certificates (Uzcard, Humo, Visa), and third-party API keys (Google AI, Telegram Bot, Protomaps) must **NEVER** be committed into source code.
- **Aspire Secret Store & Parameters**:
  - In `AppHost`, declare secrets using `builder.AddParameter("param-name", secret: true)`.
  - In development, store local secrets via .NET User Secrets (`dotnet user-secrets set ...`).
  - In production, secrets are injected via Kubernetes Secrets / HashiCorp Vault into environment variables.
- **Accessing Secrets**:
  - Reference configuration keys through `SharedKernel.ValueObjects.EnvironmentVariables` constants:
    ```csharp
    var apiKey = builder.Configuration[GOOGLE_API_KEY];
    ```

---

## 2. Network Addresses & Service Discovery

- **No Hardcoded `localhost` or IP Addresses**:
  - Never hardcode `http://localhost:50051`, `127.0.0.1:9042`, or internal microservice endpoints in application code.
- **Service Discovery**:
  - Inter-service gRPC and HTTP communication in .NET Aspire relies on service discovery names (`https+http://shopping`, `https+http://ordering`).
  - gRPC clients must be registered using `builder.Services.AddGrpcClient<TClient>(o => o.Address = new Uri("https+http://service-name"))` or `AddGrpcClients(builder.Configuration)`.
- **Public Endpoints**:
  - Public URLs must be resolved dynamically via `NavigationManager.BaseUri` (frontend) or `IConfiguration["PublicUrl"]` (backend).

---

## 3. Filesystem & Portability

- **No Absolute Local Paths**:
  - Absolute paths such as `C:\Users\...` or `/home/...` are strictly prohibited.
  - Compute relative paths using `AppContext.BaseDirectory`, `IWebHostEnvironment.ContentRootPath`, or `IWebHostEnvironment.WebRootPath`:
    ```csharp
    var modelPath = Path.Combine(AppContext.BaseDirectory, "Models", "detector.onnx");
    ```
- All path separators must use `Path.Combine()` or forward slashes `/` for Linux container compatibility.
