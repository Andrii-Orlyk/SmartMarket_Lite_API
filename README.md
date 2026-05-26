# SmartMarket Lite API

SmartMarket Lite API is a **backend-only** portfolio project for a small e-commerce flow:

```text
catalog -> cart -> checkout -> order
```

The goal is to demonstrate that the backend can model and execute a real business flow, not only isolated CRUD operations.

## What this project demonstrates

- ASP.NET Core Web API with layered architecture.
- JWT authentication and role-based access.
- Product catalog management by admin users.
- Cart logic for authenticated users.
- Checkout as one consistent operation.
- Order and order-item snapshots for historical correctness.
- EF Core + PostgreSQL relational modeling.
- Validation, unified API errors, and clear HTTP contracts.
- Unit and integration tests for critical paths.
- Docker Compose and GitHub Actions.
- Reviewer-friendly Swagger and terminal smoke testing.

## Business flow

```text
1. Admin creates active products.
2. User registers/logs in.
3. User views products.
4. User adds products to cart.
5. User updates cart quantities.
6. User checks out.
7. System creates an order with order item snapshots.
8. System decreases product stock.
9. System clears the cart.
10. User can view only own orders.
```

## Key e-commerce idea

`OrderItem` stores snapshot fields:

- `ProductNameSnapshot`
- `UnitPriceSnapshot`
- `Quantity`
- `LineTotal`

This prevents old orders from changing when product name or price changes later.

Checkout copies `CartItem.UnitPriceSnapshot` into each `OrderItem` so the purchase price is fixed at add-to-cart time. Product name is snapshotted at checkout.

Checkout runs in one database transaction: create order, create order items, decrease stock, clear cart.

## Portfolio verification

| Layer | What it proves |
|---|---|
| `dotnet test` | Unit + integration behavior (EF InMemory via `WebApplicationFactory`) |
| GitHub Actions | `dotnet restore`, `build`, and `test` on every push/PR to `main` |
| Docker Compose + `./scripts/api-smoke-test.sh` | Live PostgreSQL runtime and full `catalog -> cart -> checkout -> order` flow |

Integration tests are fast and CI-friendly but do not fully validate PostgreSQL transaction semantics. Run the smoke test manually after starting Docker Compose.

## Solution structure

```text
SmartMarket.sln

src/
  SmartMarket.Api              # ASP.NET Core Web API, controllers, middleware, auth wiring
  SmartMarket.Application      # Use cases, DTOs, validators, interfaces, result patterns
  SmartMarket.Domain           # Entities, enums, business concepts
  SmartMarket.Infrastructure   # EF Core, PostgreSQL, repositories, auth implementations

tests/
  SmartMarket.UnitTests
  SmartMarket.IntegrationTests

docs/
  API.md
  API_EXAMPLES.md
  ARCHITECTURE.md
  AUTH.md
  DATABASE.md
  DOCKER.md
  KNOWN_LIMITATIONS.md
  PROJECT_SCOPE.md
  ROADMAP.md
  SWAGGER_TESTING.md
  TERMINAL_TESTING.md
  TESTING.md

scripts/
  api-smoke-test.sh
```

## Main endpoints

### Auth

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

### Products

```http
GET    /api/products
GET    /api/products/{id}
POST   /api/admin/products
PUT    /api/admin/products/{id}
DELETE /api/admin/products/{id}
```

### Cart

```http
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
DELETE /api/cart
```

### Checkout

```http
POST /api/checkout
```

### Orders

```http
GET /api/orders
GET /api/orders/{id}
```

### Admin orders

```http
GET   /api/admin/orders
PATCH /api/admin/orders/{id}/status
```

## Run locally

```bash
dotnet restore
dotnet build
dotnet test
```

With PostgreSQL running locally, start the API in Development (applies migrations and seeds a local admin user):

```bash
cd src/SmartMarket.Api
dotnet run
```

Development seed admin (local only):

```text
Email: admin@smartmarket.local
Password: Password123!
```

Docker:

```bash
docker compose up --build
```

Swagger:

```text
http://localhost:5000/swagger
```

## Automated API smoke test

Start the API and database first, then run:

```bash
chmod +x scripts/api-smoke-test.sh
./scripts/api-smoke-test.sh
```

Custom base URL:

```bash
BASE_URL=http://localhost:5000 ./scripts/api-smoke-test.sh
```

Optional admin credentials for the smoke script:

```bash
ADMIN_EMAIL=admin@smartmarket.local ADMIN_PASSWORD=Password123! ./scripts/api-smoke-test.sh
```

The script exits with code `0` only when all required checks pass.

Verified against Docker Compose + PostgreSQL with 24/24 smoke checks. Re-run locally before publishing if you change runtime code.

## Documentation

- [Project scope](docs/PROJECT_SCOPE.md)

- [Architecture](docs/ARCHITECTURE.md)
- [API contract](docs/API.md)
- [API examples](docs/API_EXAMPLES.md)
- [Auth](docs/AUTH.md)
- [Swagger testing](docs/SWAGGER_TESTING.md)
- [Terminal testing](docs/TERMINAL_TESTING.md)
- [Docker](docs/DOCKER.md)
- [Database model](docs/DATABASE.md)
- [Testing](docs/TESTING.md)
- [Known limitations](docs/KNOWN_LIMITATIONS.md)
- [Roadmap](docs/ROADMAP.md)
