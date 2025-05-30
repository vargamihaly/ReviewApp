# ReviewApp API

This solution implements a product review system using Azure Table Storage with ASP.NET Core and Aspire orchestration. The main HTTP API is hosted in the `ReviewApp.Api` project.

The API allows:

- ✅ Managing products (CRUD operations)  
- ✅ Submitting and retrieving product reviews  
- ✅ Ensuring review consistency with optimistic concurrency control  

---

## 📚 Available Endpoints

### 🛍️ Products

| Method | Endpoint                          | Body                 | Description                                                           |
|--------|-----------------------------------|----------------------|-----------------------------------------------------------------------|
| GET    | `/api/products`                   | –                    | Returns all products.                                                 |
| GET    | `/api/products/{productName}`     | –                    | Checks if a product exists. Returns `200 OK` or `404 Not Found`.      |
| POST   | `/api/products`                   | `ProductRequest`     | Creates a new product. Requires `name` and `description`.             |
| PUT    | `/api/products/{productName}`     | `UpdateProductRequest` | Updates the description of an existing product.                       |
| DELETE | `/api/products/{productName}`     | –                    | Deletes a product. Returns `204 No Content` or `404 Not Found`.       |

### ✍️ Reviews

All review endpoints are scoped to a product name.

| Method | Endpoint                                     | Body             | Description                                                                 |
|--------|----------------------------------------------|------------------|-----------------------------------------------------------------------------|
| GET    | `/api/reviews/{productName}?limit={n}`       | –                | Retrieves the latest reviews. Optional `limit` (1–100, default: 10).        |
| POST   | `/api/reviews/{productName}`                 | `ReviewRequest`  | Submits a review. Requires `content` and `latestFetchedReviewTimestampUtc`. |

---

## 📦 Request DTOs

```csharp
public class ProductRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
}
```

```csharp
public class UpdateProductRequest
{
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = null!;
}
```

```csharp
public class ReviewRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = null!;

    [Required]
    public DateTimeOffset? LatestFetchedReviewTimestampUtc { get; set; }
}
```

All DTOs are located in `ReviewApp.Api/Dto/Request`.

---

## 🧱 Architecture

| Project                        | Responsibility                                           |
|-------------------------------|----------------------------------------------------------|
| `ReviewApp.Api`               | Web API controllers, DTOs, and HTTP routing              |
| `ReviewApp.Application`       | Core business logic, interfaces, and entities            |
| `ReviewApp.Infrastructure`    | Azure Table Storage-specific implementations             |
| `ReviewApp.AppHost`           | Aspire orchestration, service wiring, and emulation      |
| `ReviewApp.IntegrationTests`  | End-to-end tests with Testcontainers + Azurite emulator  |

---

## 🏗 Design Decisions

### 1. Azure Table Storage
- **Why**: Cost-efficient, scalable, NoSQL model fits review data well.
- **Storage layout**:
  - Products: `PartitionKey = productName`, `RowKey = METADATA`
  - Reviews:  `PartitionKey = productName`, `RowKey = reverse timestamp ticks`

### 2. Concurrency Control
- Optimistic concurrency ensures users submit reviews only after viewing the latest.
- On POST, if `LatestFetchedReviewTimestampUtc` is older than the server's latest → `409 Conflict`.

### 3. Clean Project Structure
- **Application** is framework-agnostic business logic.
- **Infrastructure** isolates Azure SDK usage.
- **API** only wires up dependencies and hosts the HTTP surface.

---

## 📝 TODOS

- [ ] Implement pagination for product listings
- [ ] Add caching layer for frequently accessed products
- [ ] Add rate limiting to prevent abuse
- [ ] Implement soft-delete for products
- [ ] Automatically delete reviews when a product is deleted (batch delete)

---

## 🚀 Production Readiness

### 🔐 Security

Add policy-based authorization:

```csharp
// Program.cs
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("RequireAuthenticated", policy => 
        policy.RequireAuthenticatedUser());
});
```

Add Secret Management: Azure Key Vault integration

### ⚡ Performance

- Add Redis caching for product metadata
- Optimize hot partition scenarios (e.g. high-volume product)

### 📈 Monitoring & Observability

- Alerts for failed review submissions
- Health checks exposed via Aspire

### 🧪 Data Consistency

Azure Table Storage is **not transactional**:
> Deleting a product doesn't cascade delete its reviews.

🔧 Suggestion:
- Batch delete reviews upon product deletion
- Or mark as "deleted" (soft-delete strategy)

---

## 🛠 Development Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** with Aspire workload
- Docker (for integration tests)

### Local Azurite Support

- Azurite is automatically available via `azStorage.RunAsEmulator()` in Aspire
- No need for manual container configuration

## 🔍 OpenAPI Specification
The OpenAPI specification is available at:
[https://localhost:7077/openapi/v1.json](https://localhost:7077/openapi/v1.json)