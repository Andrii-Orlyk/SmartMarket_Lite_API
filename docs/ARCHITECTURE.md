# Architecture

SmartMarket Lite API uses a layered architecture for a backend e-commerce API.

## Dependency direction

```text
SmartMarket.Api
  -> SmartMarket.Application
  -> SmartMarket.Infrastructure

SmartMarket.Application
  -> SmartMarket.Domain

SmartMarket.Infrastructure
  -> SmartMarket.Application
  -> SmartMarket.Domain

SmartMarket.Domain
  -> no project dependency
```

## Layers

### SmartMarket.Api

Responsibilities:

- Controllers/endpoints.
- Routing and HTTP concerns.
- Authentication and authorization setup.
- Middleware registration.
- Swagger/OpenAPI.
- Mapping application results to HTTP responses.

Controllers should stay thin. Business logic belongs in Application services.

### SmartMarket.Application

Responsibilities:

- Use cases and services.
- DTOs.
- Validation.
- Result objects.
- Repository/service interfaces.
- Business orchestration.

Main services:

- `AuthService`
- `ProductService`
- `CartService`
- `CheckoutService`
- `OrderService`

### SmartMarket.Domain

Responsibilities:

- Entities.
- Enums.
- Core business concepts.

Entities:

- `User`
- `Product`
- `Cart`
- `CartItem`
- `Order`
- `OrderItem`

### SmartMarket.Infrastructure

Responsibilities:

- EF Core DbContext.
- Entity configurations.
- PostgreSQL repositories.
- Unit of Work.
- JWT token service implementation.
- Password hashing implementation.

## Main business flow

```text
Client
  -> POST /api/checkout
  -> CheckoutController
  -> CheckoutService
  -> CartRepository loads current user's cart
  -> ProductRepository validates active products and stock
  -> OrderRepository creates order and order items
  -> Product stock decreases
  -> Cart clears
  -> UnitOfWork commits transaction
  -> OrderResponse returned
```

## Why checkout is special

Checkout must be atomic. The API must avoid states such as:

- order created but cart not cleared;
- stock reduced but order not created;
- order total not matching order items;
- order history changing after product price changes.

For v1, EF Core transaction through Unit of Work is enough.

## Catalog, cart, and ownership rules

- Public catalog returns active products only.
- Cart is created lazily on first add-to-cart.
- Adding the same product again increases quantity on one cart line.
- Checkout re-validates stock because inventory can change after items were added.
- Users can list and read only their own orders; another user's order id returns `404`.
- Admins manage products and can list or update all orders.
