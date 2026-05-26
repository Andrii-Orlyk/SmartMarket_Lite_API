# Testing Strategy

## Test layers

```text
Unit tests -> business rules and calculations
Integration tests -> real API behavior through WebApplicationFactory
Smoke test -> reviewer-friendly terminal API flow
```

## Unit tests

Cart:

- cannot add zero quantity;
- adding same product increases quantity;
- cart total calculation;
- inactive product cannot be added;
- stock validation.

Checkout:

- cannot checkout empty cart;
- cannot checkout insufficient stock;
- order total calculation;
- order item snapshot is created;
- product stock decreases;
- cart is cleared.

Orders:

- user sees only own orders;
- invalid status transition fails.

## Integration tests

Auth:

- register/login;
- me with token;
- protected endpoints without JWT return 401 `auth.unauthorized`.

Admin products:

- admin creates product;
- non-admin cannot create product (`403 auth.forbidden`);
- invalid product price returns 400.

Cart:

- user adds product to cart;
- user updates cart item quantity;
- user removes cart item and cart no longer contains that line;
- user clears cart and receives an empty cart.

Checkout:

- checkout creates order;
- checkout clears cart;
- stock decreases;
- order item stores price snapshot;
- order item keeps cart `UnitPriceSnapshot` when product price changes before checkout;
- user cannot see another user's order.

Orders/admin:

- normal user cannot list admin orders (`403 auth.forbidden`);
- invalid order status transition returns `409` with `order.invalid_status_transition`.

## Integration test database

Integration tests run against EF Core InMemory through `WebApplicationFactory` for fast CI-friendly runs. They do not replace PostgreSQL validation for transactions, constraints, or migrations.

## Terminal smoke test

The live smoke test is manual. Run it only after the API and PostgreSQL are started (Docker Compose or local database):

```bash
chmod +x scripts/api-smoke-test.sh
./scripts/api-smoke-test.sh
```

See [Terminal testing](TERMINAL_TESTING.md) for environment variables and full scenario list.

## Current automated coverage

`dotnet test` runs unit and integration suites. GitHub Actions runs the same build and test commands on push and pull requests to `main`.

Automated suites cover auth, catalog, cart, checkout, and order access rules described in this document. A passing CI run does not replace the live smoke test against Docker/PostgreSQL.
