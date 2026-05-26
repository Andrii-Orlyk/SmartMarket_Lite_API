# Terminal API Smoke Test

The smoke test script validates the full API flow from the terminal in one command.

Script: `scripts/api-smoke-test.sh`

Requirements: `curl`, `jq`

## Run

Start API and PostgreSQL:

```bash
docker compose up --build
```

In another terminal:

```bash
chmod +x scripts/api-smoke-test.sh
./scripts/api-smoke-test.sh
```

Custom base URL:

```bash
BASE_URL=http://localhost:5000 ./scripts/api-smoke-test.sh
```

Admin credentials (defaults match Development seed):

```bash
ADMIN_EMAIL=admin@smartmarket.local ADMIN_PASSWORD=Password123! ./scripts/api-smoke-test.sh
```

The script prints `PASS`, `FAIL`, and `WARN` lines and exits `0` only when there are zero failures.

This flow was verified against Docker Compose + PostgreSQL (24/24 smoke checks passed). Re-run the script after local changes before claiming runtime readiness.

## Positive flow covered

1. Swagger JSON available.
2. Admin login.
3. User register and login.
4. `GET /api/auth/me`.
5. Admin creates active product.
6. Public product list contains the product.
7. User adds product to cart.
8. User updates cart item quantity.
9. User checks out.
10. User gets order by id.
11. User gets order history.
12. Product stock decreased after checkout.
13. Cart is empty after checkout.

## Negative flow covered

1. Missing JWT on protected endpoint -> 401.
2. Invalid login -> 401 `auth.invalid_credentials`.
3. User creates admin product -> 403 `auth.forbidden`.
4. Invalid product price -> 400.
5. Add inactive product to cart -> 409 `product.inactive`.
6. Add nonexistent product -> 404 `product.not_found`.
7. Quantity `0` -> 400.
8. Checkout empty cart -> 409 `checkout.empty_cart`.
9. Access another user's order -> 404 `order.not_found`.
10. Unknown product id on public catalog -> 404.

## Auth note

Terminal requests use:

```http
Authorization: Bearer <jwt-token>
```

Swagger UI Authorize expects only the raw JWT value, without the `Bearer` prefix.
