---
name: socratic-backend
description: Comprehensive backend architecture guide for Socratic microservices (Identifying, Shopping, Ordering, Paying, Map). USE FOR: authoring ScyllaDB NoSQL models, CQL queries, prepared statements, and repositories using Cassandra.ISession; authoring/modifying gRPC Protobuf (.proto) contracts, streaming RPCs, and gRPC-Web clients; configuring .NET Aspire orchestration, AppHost.cs, ServiceDefaults, OpenTelemetry, and container bindings; managing MinIO S3 media buckets and Qdrant vector biometrics/embeddings. DO NOT USE FOR: Entity Framework Core, SQL relational joins, or client-side Blazor UI styling.
---

# Socratic Backend Architecture: ScyllaDB, gRPC, Aspire, MinIO & Qdrant

All backend services in Socratic reside in `src/Backend/` (`Identifying`, `Shopping`, `Ordering`, `Paying`, `Map`) and inherit shared infrastructure from `src/Shared/` (`SharedKernel`, `AppHost`, `ServiceDefaults`).

---

## 1. 🏛️ ScyllaDB NoSQL Rules & CQL Patterns

> **CRITICAL RULE**: The Socratic backend runs on **ScyllaDB** (distributed C++ NoSQL, Cassandra-compatible) via the official C# `Cassandra.ISession` driver.
> **NEVER** introduce Entity Framework Core (`DbContext`, `DbSet`, migrations).
> **NEVER** write relational `JOIN` queries. Data is denormalized and queried by partition key.

### Query Design & Prepared Statements
- Every read query **must** specify the full partition key (`WHERE organization_id = ? AND id = ?`).
- In multi-tenant queries, **always include `organization_id`** to prevent cross-tenant data leakage.
- Always use `PreparedStatement` cached per service lifetime to maximize ScyllaDB shard utilization and avoid CQL injection.
- Never use `ALLOW FILTERING` on unbounded tables in production.

### Repository Implementation Pattern
```csharp
public class ScyllaProductRepository : IProductRepository
{
    private readonly ISession _session;
    private PreparedStatement _getByIdStmt = null!;
    private PreparedStatement _insertStmt = null!;

    public ScyllaProductRepository(ISession session)
    {
        _session = session;
    }

    public async Task InitializeStatementsAsync()
    {
        _getByIdStmt = await _session.PrepareAsync(
            "SELECT id, organization_id, name, price, product_mode, metadata FROM products WHERE organization_id = ? AND id = ?");
            
        _insertStmt = await _session.PrepareAsync(
            "INSERT INTO products (id, organization_id, name, price, product_mode, metadata, created_at) " +
            "VALUES (?, ?, ?, ?, ?, ?, toTimestamp(now()))");
    }

    public async Task<Product?> GetByIdAsync(Guid orgId, Guid id)
    {
        var bound = _getByIdStmt.Bind(orgId, id);
        var rowSet = await _session.ExecuteAsync(bound);
        var row = rowSet.FirstOrDefault();
        return row != null ? MapRowToProduct(row) : null;
    }
}
```

### Programmatic Schema Evolution
Schema keyspaces and tables are ensured during service startup:
```csharp
public static async Task EnsureSchemaAsync(ISession session, string keyspace)
{
    await session.ExecuteAsync(new SimpleStatement(
        $"CREATE KEYSPACE IF NOT EXISTS {keyspace} WITH replication = {{'class': 'SimpleStrategy', 'replication_factor': 1}};"));

    await session.ExecuteAsync(new SimpleStatement(
        $"CREATE TABLE IF NOT EXISTS {keyspace}.products (" +
        "organization_id uuid," +
        "id uuid," +
        "name text," +
        "price decimal," +
        "product_mode int," +
        "metadata text," +
        "created_at timestamp," +
        "PRIMARY KEY ((organization_id), id));"));
}
```

---

## 2. ⚡ gRPC & Protocol Buffers Workflow

Remote procedure calls between microservices and client apps use **gRPC** and **gRPC-Web**.

### Proto File Locations
- `src/Shared/SharedKernel/Protos/`: `File.proto`, `Options.proto`, `Name.proto`, `Address.proto`
- `src/Backend/Identifying/Identifying.Application/Protos/`: `Auth.proto`, `User.proto`, `Profile.proto`
- `src/Backend/Shopping/Shopping.Application/Protos/`: `Product.proto`, `Organization.proto`, `Layout.proto`
- `src/Backend/Ordering/Ordering.Application/Protos/`: `Order.proto`, `OrderItem.proto`
- `src/Backend/Paying/Paying.Application/Protos/`: `Payment.proto`

### Contract Example
```protobuf
syntax = "proto3";
option csharp_namespace = "Shopping.Application.Protos";
package shopping;

message ProductDto {
  string id = 1;
  string organization_id = 2;
  string name = 3;
  double price = 4;
  int32 product_mode = 5;
}

service ProductService {
  rpc Read (ProductDto) returns (stream ProductDto);
  rpc Create (ProductDto) returns (ProductDto);
  rpc Update (ProductDto) returns (ProductDto);
  rpc Delete (ProductDto) returns (ProductDto);
}
```

### gRPC Service Implementation
```csharp
public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private readonly IProductRepository _repository;
    public ProductServiceImpl(IProductRepository repository) => _repository = repository;

    public override async Task Read(ProductDto request, IServerStreamWriter<ProductDto> responseStream, ServerCallContext context)
    {
        var items = await _repository.GetAllAsync(Guid.Parse(request.OrganizationId));
        foreach (var item in items)
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            await responseStream.WriteAsync(item.ToDto());
        }
    }
}
```

### Client Registration & Blazor Consumption (gRPC-Web)
In `Program.cs`:
```csharp
builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:Shopping:Https"]!);
}).ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));
```

In Blazor:
```razor
@inject ProductService.ProductServiceClient ProductClient

@code {
    protected override async Task OnInitializedAsync()
    {
        using var call = ProductClient.Read(new ProductDto { OrganizationId = OrgId.ToString() });
        await foreach (var product in call.ResponseStream.ReadAllAsync())
        {
            Products.Add(product);
            StateHasChanged();
        }
    }
}
```

---

## 3. 🌐 .NET Aspire Orchestration

Distributed orchestration, service discovery, container dependencies, and telemetry are managed via `src/Shared/AppHost/`.

### AppHost Resource Graph
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var minio = builder.AddMinio(parameters, release: false);
var qdrant = builder.AddQdrant(parameters, release: false);
var scylla = builder.AddScylla(parameters, release: false);

var identifying = builder.AddIdentifying(parameters, release)
    .WithReference(scylla).WithReference(minio);

var shopping = builder.AddShopping(parameters, release)
    .WithReference(scylla).WithReference(minio).WithReference(qdrant);

var ordering = builder.AddOrdering(parameters, release).WithReference(scylla);
var paying = builder.AddPaying(parameters, release).WithReference(scylla);
```

### Service Defaults
All services include OpenTelemetry, health checks, and resilient gRPC discovery:
```csharp
builder.AddServiceDefaults();
...
app.MapDefaultEndpoints(); // Exposes /health, /alive, and OpenTelemetry OTLP
```

---

## 4. 🗄️ MinIO (Object Storage) & Qdrant (Vector DB)

Unstructured media and AI embeddings are kept outside the transactional ScyllaDB:
- **MinIO**: S3-compatible storage for product images, logos, avatars, and audio (`products`, `organizations`, `avatars`, `biometrics` buckets).
- **Qdrant**: Vector database for AI Face ID matching (512-dim ArcFace embeddings, Cosine distance) and product semantic search.

```csharp
// Qdrant Face Matching Example
public async Task<ulong?> FindMatchingUserAsync(QdrantClient qdrant, float[] probeEmbedding, float threshold = 0.72f)
{
    var results = await qdrant.SearchAsync("face_embeddings", probeEmbedding, limit: 1);
    var best = results.FirstOrDefault();
    return (best != null && best.Score >= threshold) ? best.Id.Num : null;
}
```
