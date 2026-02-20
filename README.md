# Order Management API

A RESTful API for managing customer orders and order items, built with .NET 10 and following Clean Architecture principles.

## Architecture

The solution is organized into four layers:

```
OrderManagementApi/
├── Orm.Api                  # Presentation layer (Controllers, Program.cs)
├── Orm.Application          # Application layer (CQRS Commands/Queries, DTOs, Mapper)
├── Orm.Domain               # Domain layer (Entities, Repository interfaces)
├── Orm.Infrastructure       # Infrastructure layer (EF Core DbContext, Repositories)
├── Orm.Application.Tests    # Unit tests for Application layer
├── Orm.Infrastructure.Tests # Integration tests for Infrastructure layer
└── Orm.Api.Tests            # Unit tests for Api layer
```

- **Orm.Domain** contains the core entities (`Order`, `OrderItem`) and repository interfaces. It has no dependencies on other layers.
- **Orm.Application** implements the **CQRS pattern** using **MediatR**. Commands (`CreateOrder`, `UpdateOrder`, `DeleteOrder`) and Queries (`GetOrderById`) are organized into dedicated folders, each with their own request and handler classes. Also contains DTOs and the `IMapper`/`Mapper` for entity-DTO mapping. Depends only on `Orm.Domain`.
- **Orm.Infrastructure** implements data access with Entity Framework Core and SQL Server. Contains `AppDbContext` and `OrderRepository`. Depends on `Orm.Domain` and `Orm.Application`.
- **Orm.Api** is the entry point. The `OrderController` dispatches requests through `IMediator` rather than calling services directly. Depends on `Orm.Application` and `Orm.Infrastructure`.

## Tech Stack

- **.NET 10** / ASP.NET Core
- **MediatR** for CQRS and Mediator pattern
- **Entity Framework Core 10** with SQL Server
- **Scalar** for interactive API documentation
- **xUnit** + **Moq** for testing
- **EF Core InMemory** provider for integration tests

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local or remote)

### Configuration

Update the connection string in `Orm.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlConnectionString": "Server=YOUR_SERVER;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Run the API

```bash
dotnet run --project Orm.Api
```

The API will be available at `http://localhost:5096`. In development mode, the interactive Scalar API documentation is served at `/scalar/v1`.

## API Endpoints

All endpoints are under the base path `/api/v1/order`.

| Method | Route                          | Description              | Success |
|--------|--------------------------------|--------------------------|---------|
| GET    | `/api/v1/order/get-order/{id}` | Get an order by ID       | 200 OK  |
| POST   | `/api/v1/order/create-order`   | Create a new order       | 201 Created |
| PUT    | `/api/v1/order/update-order/{id}` | Update an existing order | 200 OK  |
| DELETE | `/api/v1/order/delete-order/{id}` | Delete an order          | 200 OK  |

### Request/Response Examples

**Create Order** `POST /api/v1/order/create-order`

```json
{
  "customerName": "Jane Doe",
  "orderItems": [
    { "productId": 1, "quantity": 2, "price": 29.99 },
    { "productId": 5, "quantity": 1, "price": 9.50 }
  ]
}
```

**Response** `201 Created`

```json
{
  "orderID": 1,
  "createDate": "2026-02-17T12:00:00Z",
  "customerName": "Jane Doe",
  "orderItems": [
    { "orderItemID": 1, "orderID": 1, "productId": 1, "quantity": 2, "price": 29.99 },
    { "orderItemID": 2, "orderID": 1, "productId": 5, "quantity": 1, "price": 9.50 }
  ]
}
```

**Update Order** `PUT /api/v1/order/update-order/1`

```json
{
  "orderID": 1,
  "customerName": "Jane Doe",
  "orderItems": [
    { "productId": 1, "quantity": 3, "price": 29.99 },
    { "productId": 10, "quantity": 1, "price": 14.00 }
  ]
}
```

The update logic matches items by `productId`: existing items are updated, missing items are removed, and new items are added.

## Testing

Run all tests from the solution root:

```bash
dotnet test
```

The test suite includes 25 tests across three projects:

| Project | Type | What's Tested |
|---------|------|---------------|
| **Orm.Application.Tests** | Unit (Moq) | `Mapper` entity/DTO mapping, CQRS command and query handlers |
| **Orm.Infrastructure.Tests** | Integration (EF InMemory) | `OrderRepository` CRUD operations |
| **Orm.Api.Tests** | Unit (Moq) | `OrderController` HTTP responses and status codes (mocking `IMediator`) |

## License

This project is provided as-is for educational and demonstration purposes.
