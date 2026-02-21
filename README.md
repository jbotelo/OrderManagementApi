# Order Management API

A RESTful API for managing customer orders and order items, built with .NET 10 and following Clean Architecture principles. Features JWT Bearer authentication with ASP.NET Core Identity, refresh token rotation, and role-based authorization.

## Architecture

The solution is organized into four layers:

```
OrderManagementApi/
├── Orm.Api                  # Presentation layer (Controllers, Program.cs)
├── Orm.Application          # Application layer (CQRS Commands/Queries, DTOs, Mapper)
├── Orm.Domain               # Domain layer (Entities, Repository interfaces)
├── Orm.Infrastructure       # Infrastructure layer (EF Core DbContext, Repositories, Services)
├── Orm.Application.Tests    # Unit tests for Application layer
├── Orm.Infrastructure.Tests # Integration tests for Infrastructure layer
└── Orm.Api.Tests            # Unit tests for Api layer
```

- **Orm.Domain** contains the core entities (`Order`, `OrderItem`, `ApplicationUser`, `RefreshToken`) and repository interfaces. It has no dependencies on other layers.
- **Orm.Application** implements the **CQRS pattern** using **MediatR**. Commands (`CreateOrder`, `UpdateOrder`, `DeleteOrder`, `Register`, `Login`, `RefreshToken`) and Queries (`GetOrderById`) are organized into dedicated folders, each with their own request and handler classes. Also contains DTOs, the `IMapper`/`Mapper` for entity-DTO mapping, `ITokenService`, and authorization policy constants. Depends only on `Orm.Domain`.
- **Orm.Infrastructure** implements data access with Entity Framework Core and SQL Server. Contains `AppDbContext` (extending `IdentityDbContext`), `OrderRepository`, `RefreshTokenRepository`, and `TokenService` for JWT generation. Depends on `Orm.Domain` and `Orm.Application`.
- **Orm.Api** is the entry point. The `OrderController` dispatches requests through `IMediator` rather than calling services directly. The `AuthController` handles registration, login, and token refresh. Depends on `Orm.Application` and `Orm.Infrastructure`.

## Tech Stack

- **.NET 10** / ASP.NET Core
- **MediatR** for CQRS and Mediator pattern
- **Entity Framework Core 10** with SQL Server
- **ASP.NET Core Identity** for user management
- **JWT Bearer Authentication** with refresh token rotation
- **Scalar** for interactive API documentation
- **xUnit** + **Moq** for testing
- **EF Core InMemory** provider for integration tests

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local or remote)

### Configuration

Update the connection string in `Orm.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqlConnectionString": "Server=YOUR_SERVER;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For production, configure the JWT secret key via environment variables or a secrets manager. The default key in `appsettings.json` is for development only:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "OrderManagementApi",
    "Audience": "OrderManagementApiClients",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Database Setup

Apply EF Core migrations to create the database schema:

```bash
dotnet ef database update --project Orm.Infrastructure --startup-project Orm.Api
```

### Run the API

```bash
dotnet run --project Orm.Api
```

The API will be available at `http://localhost:5096`. In development mode, the interactive Scalar API documentation is served at `/scalar/v1`.

## Authentication

The API uses JWT Bearer authentication. Three roles are seeded at startup: **Admin**, **Manager**, and **User**.

### Auth Endpoints

All auth endpoints are under `/api/v1/auth` and do not require authentication.

| Method | Route                          | Description                        |
|--------|--------------------------------|------------------------------------|
| POST   | `/api/v1/auth/register`        | Register a new user (assigned User role) |
| POST   | `/api/v1/auth/login`           | Log in and receive tokens          |
| POST   | `/api/v1/auth/refresh-token`   | Exchange a refresh token for new tokens |

### Auth Flow

1. **Register** or **Login** to receive an access token (JWT, 15 min) and a refresh token (7 days)
2. Include the access token in the `Authorization` header: `Bearer <access_token>`
3. When the access token expires, call `/refresh-token` with the refresh token to get a new pair
4. The old refresh token is revoked on each rotation for security

**Register** `POST /api/v1/auth/register`

```json
{
  "email": "user@example.com",
  "password": "MyPassword1!",
  "fullName": "John Doe"
}
```

**Login** `POST /api/v1/auth/login`

```json
{
  "email": "user@example.com",
  "password": "MyPassword1!"
}
```

**Response** (both register and login)

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-random-string",
  "accessTokenExpiration": "2026-02-21T12:15:00Z"
}
```

**Refresh Token** `POST /api/v1/auth/refresh-token`

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-random-string"
}
```

### Authorization Policies

Order endpoints require authentication and are protected by role-based policies:

| Policy | Endpoint | Allowed Roles |
|--------|----------|---------------|
| `Orders.Read` | GET order | User, Manager, Admin |
| `Orders.Create` | POST create order | Manager, Admin |
| `Orders.Update` | PUT update order | Manager, Admin |
| `Orders.Delete` | DELETE order | Admin only |

## Order Endpoints

All order endpoints are under `/api/v1/order` and require authentication.

| Method | Route                          | Description              | Success | Required Role |
|--------|--------------------------------|--------------------------|---------|---------------|
| GET    | `/api/v1/order/get-order/{id}` | Get an order by ID       | 200 OK  | User+ |
| POST   | `/api/v1/order/create-order`   | Create a new order       | 201 Created | Manager+ |
| PUT    | `/api/v1/order/update-order/{id}` | Update an existing order | 200 OK  | Manager+ |
| DELETE | `/api/v1/order/delete-order/{id}` | Delete an order          | 200 OK  | Admin |

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

The test suite includes 46 tests across three projects:

| Project | Type | What's Tested |
|---------|------|---------------|
| **Orm.Application.Tests** | Unit (Moq) | `Mapper` entity/DTO mapping, CQRS command/query handlers, auth command handlers (Register, Login, RefreshToken) |
| **Orm.Infrastructure.Tests** | Integration (EF InMemory) | `OrderRepository` CRUD operations, `RefreshTokenRepository` CRUD and revocation |
| **Orm.Api.Tests** | Unit (Moq) | `OrderController` and `AuthController` HTTP responses and status codes (mocking `IMediator`) |

## License

This project is provided as-is for educational and demonstration purposes.
