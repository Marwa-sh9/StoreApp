# StoreApp – Store Inventory API

A RESTful Web API built with **C# and ASP.NET Core (.NET 8)** for managing a store's **product catalog and categories**. The sample data is themed around a clear-aligner / dental-care store, but the design fits any small inventory system.

## Problem & Solution

Small stores need a reliable way to keep track of what they sell, how it is categorized, and how much is in stock, without losing historical data when something is removed.

This API provides:

- Full CRUD for **Categories** and **Products**
- **Soft delete**, so records are never physically lost
- **Unique SKU** enforcement (even for deleted products)
- Business rules that protect data integrity (e.g. a category with active products cannot be deleted)
- **Input validation** and **consistent error responses**

---

## Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 8) |
| Language | C# |
| ORM | Entity Framework Core 8 (Code First + Migrations) |
| Database | SQL Server (LocalDB / SQL Server Express) |
| Validation | FluentValidation |
| API Docs | Swagger / OpenAPI (Swashbuckle) |

---

## Architecture

The solution follows a **layered (clean) architecture**. Dependencies point inward: the API depends on Application, Infrastructure implements Application's interfaces, and Domain depends on nothing.

```
StoreApp.sln
├── Store.Domain/            # Entities (Category, Product). No dependencies.
├── StoreApp.Application/    # Business logic
│   ├── DTOs/                # Request/response models
│   ├── Interfaces/          # Service & repository contracts
│   ├── Services/            # Business rules
│   ├── Validators/          # FluentValidation rules
│   └── Exceptions/          # NotFoundException, ConflictException
├── StoreApp.Infrastructure/ # Data access
│   ├── ApplicationDbContext.cs
│   ├── Repositories/        # EF Core implementations
│   ├── Data/DbSeeder.cs     # Applies migrations + seeds sample data
│   └── Migrations/
└── StoreApp.Api/            # HTTP layer
    ├── Controllers/         # Thin controllers
    ├── Filters/             # ValidationFilter
    ├── Middleware/          # ErrorHandlingMiddleware
    └── Program.cs           # DI & pipeline configuration
```

**Request flow**

```
HTTP request
  → ErrorHandlingMiddleware (catches any exception)
  → ValidationFilter (runs FluentValidation on the DTO → 400 if invalid)
  → Controller (thin, delegates only)
  → Service (business rules, throws NotFound/Conflict exceptions)
  → Repository (EF Core queries)
  → SQL Server
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server, SQL Server Express, or LocalDB (installed with Visual Studio)
- (Optional) EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### 1. Clone the repository

```bash
git clone https://github.com/Marwa-sh9/StoreApp.git
cd StoreApp
```

### 2. Configure the connection string

Open `StoreApp.Api/appsettings.json` and make sure `DefaultConnection` points to your SQL Server instance. Example for LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StoreInventoryDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

> Do not commit real passwords. Prefer Windows Authentication, `dotnet user-secrets`, or environment variables.

### 3. Run the application

```bash
dotnet run --project StoreApp.Api
```

On startup the app automatically:

1. Creates the database (if it does not exist) and **applies all migrations**
2. **Seeds** 3 categories and 6 sample products (only if the tables are empty)

No manual `dotnet ef database update` is required.

### 4. Open Swagger

Go to `http://localhost:5205/swagger` (the exact port is printed in the console when the app starts).

### Useful EF Core commands (optional)

```bash
# Add a new migration after changing the model
dotnet ef migrations add <Name> --project StoreApp.Infrastructure --startup-project StoreApp.Api

# Apply migrations manually
dotnet ef database update --project StoreApp.Infrastructure --startup-project StoreApp.Api
```

---

## API Endpoints

### Categories – `/api/categories`

| Method | Endpoint | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/categories` | List all active categories | 200 | – |
| GET | `/api/categories/{id}` | Get a category by id | 200 | 404 |
| POST | `/api/categories` | Create a category | 201 | 400 |
| PUT | `/api/categories/{id}` | Update a category | 204 | 400, 404 |
| DELETE | `/api/categories/{id}` | Soft-delete a category | 204 | 404, **409** if it has active products |

### Products – `/api/products`

| Method | Endpoint | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/products` | List all active products (with category name) | 200 | – |
| GET | `/api/products/{id}` | Get a product by id | 200 | 404 |
| POST | `/api/products` | Create a product | 201 | 400, 404 (category), **409** (duplicate SKU) |
| PUT | `/api/products/{id}` | Update a product | 204 | 400, 404, **409** |
| DELETE | `/api/products/{id}` | Soft-delete a product | 204 | 404 |

### Example request

`POST /api/products`

```json
{
  "name": "Complete Aligner Cleaning Kit",
  "sku": "CARE-003",
  "price": 35.00,
  "quantityInStock": 40,
  "categoryId": 2
}
```

---

## Validation Rules

**Category**

| Field | Rule |
|---|---|
| Name | Required, max 100 characters |
| Description | Optional, max 500 characters |

**Product**

| Field | Rule |
|---|---|
| Name | Required, max 150 characters |
| SKU | Required, max 50 characters, unique |
| Price | Must be greater than 0 |
| QuantityInStock | Must be 0 or greater |
| CategoryId | Must be greater than 0 and reference an existing, non-deleted category |

---

## Error Handling

All errors are returned in the standard **RFC 7807 `ProblemDetails`** format, so clients get one consistent shape.

| Situation | Status | Source |
|---|---|---|
| Invalid input | 400 | `ValidationFilter` (returns field-level errors) |
| Resource not found | 404 | `NotFoundException` |
| Duplicate SKU / category still has products | 409 | `ConflictException` |
| Unexpected error | 500 | Generic message only (details are logged, never exposed) |

Example (404):

```json
{
  "title": "Resource not found",
  "status": 404,
  "detail": "Product not found.",
  "instance": "/api/products/999"
}
```

---

## Key Technical Decisions

### 1. Layered architecture with Repository + Service
Controllers only handle HTTP concerns. Business rules live in services, and data access is hidden behind repository interfaces. This keeps each class focused, makes the code easy to unit test (repositories can be mocked), and allows replacing the data layer without touching business logic.

### 2. DTOs instead of exposing entities
Entities are never returned directly. DTOs control exactly what the API accepts and exposes, and prevent over-posting (e.g. a client setting `IsDeleted`).

### 3. Soft delete with a global query filter
Records are flagged (`IsDeleted`, `DeletedAt`) instead of being removed. A global EF Core query filter (`HasQueryFilter(x => !x.IsDeleted)`) hides deleted rows from every query automatically, so no query can forget to filter them.

### 4. SKU uniqueness includes deleted products
A unique database index on `SKU` guarantees uniqueness. The service checks for duplicates first using `IgnoreQueryFilters()` so soft-deleted products are also considered. This returns a clean **409** instead of a database error. The repository also converts a unique-constraint violation into a `ConflictException` to handle the race condition where two requests use the same SKU at the same time.

### 5. `DeleteBehavior.Restrict` for Category → Product
Cascade delete is dangerous when combined with soft delete. The database refuses to remove a category that still has products, and the service additionally returns a friendly 409 when a category still has **active** products.

### 6. Efficient existence check
`AnyByCategoryIdAsync` runs `SELECT EXISTS(...)` in the database instead of loading every product into memory.

### 7. FluentValidation through a global action filter
Validation rules are kept out of controllers and DTOs. One `ValidationFilter` runs the matching validator for each action argument, which avoids duplicated validation code in every action.

### 8. Centralized error handling middleware
Services throw meaningful exceptions (`NotFoundException`, `ConflictException`) and a single middleware maps them to HTTP status codes and `ProblemDetails`. It is registered first in the pipeline so it catches everything, logs errors, and never leaks internal details on 500 errors.

### 9. EF Core tracking strategy
Read-only queries use `AsNoTracking()` for performance. Entities fetched for modification stay tracked, so changing properties and calling `SaveChangesAsync()` produces an `UPDATE` for only the changed columns (no explicit `Update()` call needed).

### 10. Automatic migration and seeding on startup
`DbSeeder` applies pending migrations and inserts sample data when the database is empty, so the project runs with a single command. For a production system, migrations would normally be applied as a separate deployment step instead.

---

## Possible Improvements

- Pagination, sorting, and filtering (e.g. products by category)
- Unit tests for services (xUnit + Moq) and integration tests
- Authentication and authorization (JWT)
- `UpdatedAt` audit field
- Docker support
- Separate `UpdateProductDto` / `UpdateCategoryDto` with their own validators

---

## Author

**Marwa** – Technical Assessment for Junior Software Engineer
