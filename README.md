# Store Management Web API

A robust, RESTful Web API built with **ASP.NET Core** and **C#**, designed following **Clean Architecture** principles and software design best practices. The application provides complete CRUD operations for managing product categories and products with built-in validation, soft deletion, and centralized error handling.

---

## 🛠️ Tech Stack & Key Packages

- **Framework:** .NET 8.0 / ASP.NET Core Web API
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core 8.0
- **Validation:** FluentValidation
- **Documentation:** Swagger / OpenAPI
- **Architecture Pattern:** Clean Architecture (Domain, Application, Infrastructure, API)

---

## 🏗️ Architectural & Technical Decisions

1. **Clean Architecture (Separation of Concerns):**
   - **Domain:** Contains core entities (`Product`, `Category`) without external dependencies.
   - **Application:** Contains DTOs, Repository/Service Interfaces, Custom Exceptions (`NotFoundException`, `ConflictException`), and FluentValidation rules.
   - **Infrastructure:** Handles EF Core `ApplicationDbContext`, Migrations, Data Seeding, and Repository implementations.
   - **API:** Contains API Controllers, Middleware, and Dependency Injection registration.

2. **Repository & Service Patterns:**
   Decouples business logic from data access code, facilitating maintainability and unit testability.

3. **Global Exception Handling Middleware:**
   Centralized middleware (`ErrorHandlingMiddleware`) catches all unhandled exceptions, mapping custom domain exceptions (`NotFoundException` -> `404 Not Found`, `ConflictException` -> `409 Conflict`) to standardized HTTP JSON responses.

4. **Input Validation:**
   Utilizes **FluentValidation** to enforce strict data contracts on incoming DTOs prior to domain processing.

5. **Soft Delete Mechanism:**
   Entities feature `IsDeleted` and `DeletedAt` fields to prevent physical loss of historical data while keeping queries filtered.

6. **Unique Constraints & Referential Integrity:**
   - SKU uniqueness is enforced both at the database index level and within application service validation.
   - Deletion of categories containing active products is prevented to enforce referential consistency.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express / LocalDB

### Configuration & Setup

1. **Clone the Repository:**
   ```bash
   git clone [https://github.com/YOUR_USERNAME/StoreApp.git](https://github.com/YOUR_USERNAME/StoreApp.git)
   cd StoreApp
