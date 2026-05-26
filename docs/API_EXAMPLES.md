# API Examples

These examples are split into happy-path scenarios and expected failures.

Base URL:

```text
http://localhost:5000
```

## Auth rule

For curl/Postman:

```http
Authorization: Bearer <jwt-token>
```

For Swagger UI Authorize popup, paste only the raw JWT token without `Bearer`.

## Placeholder rule

Do not paste example IDs directly. Replace placeholders with real values returned by previous responses:

- `<jwt-token>` from login/register.
- `<productId>` from admin product creation.
- `<cartItemId>` from add-to-cart response.
- `<orderId>` from checkout response.

# Part 1 — Positive Scenarios / Happy Path

For admin steps in local Development, log in with the seeded admin:

```http
POST /api/auth/login
```

```json
{
  "email": "admin@smartmarket.local",
  "password": "Password123!"
}
```

## 1. Register

```http
POST /api/auth/register
```

```json
{
  "email": "buyer@example.com",
  "password": "Password123!",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

Expected: `200 OK` with token.

## 2. Login

```http
POST /api/auth/login
```

Expected: `200 OK` with token.

## 3. Me

```http
GET /api/auth/me
Authorization: Bearer <jwt-token>
```

Expected: `200 OK` with current user data.

## 4. Admin creates product

```http
POST /api/admin/products
Authorization: Bearer <admin-jwt-token>
```

```json
{
  "name": "Wireless Mouse",
  "description": "Compact wireless mouse",
  "sku": "MOUSE-001",
  "price": 49.99,
  "stockQuantity": 25,
  "isActive": true
}
```

Expected: `200 OK`. Save returned `id` as `<productId>`.

## 5. Get products

```http
GET /api/products
```

Expected: `200 OK`. Response contains active products.

## 6. Add product to cart

```http
POST /api/cart/items
Authorization: Bearer <jwt-token>
```

```json
{
  "productId": "<productId>",
  "quantity": 2
}
```

Expected: `200 OK`. Save returned cart item `id` as `<cartItemId>`.

## 7. Update cart item quantity

```http
PUT /api/cart/items/<cartItemId>
Authorization: Bearer <jwt-token>
```

```json
{
  "quantity": 3
}
```

Expected: `200 OK`.

## 8. Checkout

```http
POST /api/checkout
Authorization: Bearer <jwt-token>
```

Expected: `200 OK`. Save returned order `id` as `<orderId>`.

Expected behavior:

- order is created;
- order items are created;
- product name/price snapshots are stored;
- product stock decreases;
- cart is cleared.

## 9. Get order

```http
GET /api/orders/<orderId>
Authorization: Bearer <jwt-token>
```

Expected: `200 OK`.

## 10. Get order history

```http
GET /api/orders
Authorization: Bearer <jwt-token>
```

Expected: `200 OK`.

# Part 2 — Negative Scenarios / Expected Failures

Run these only to verify error handling.

| Scenario | Expected |
|---|---|
| Missing JWT on protected endpoint | 401 `auth.unauthorized` |
| Invalid login | 401 `auth.invalid_credentials` |
| Normal user creates product | 403 `auth.forbidden` |
| Product with price <= 0 | 400 `validation.failed` or `product.invalid_price` |
| Product with stock < 0 | 400 `validation.failed` or `product.invalid_stock` |
| Add inactive product to cart | 409 `product.inactive` |
| Add nonexistent product to cart | 404 `product.not_found` |
| Quantity <= 0 | 400 `cart.invalid_quantity` |
| Checkout empty cart | 409 `checkout.empty_cart` |
| Checkout insufficient stock | 409 `checkout.insufficient_stock` |
| Access another user's order | 404 `order.not_found` |
| Normal user lists admin orders | 403 `auth.forbidden` |
| Invalid order status transition | 409 `order.invalid_status_transition` |
| Unhandled server exception | 500 `server.error` |

# Part 3 — Troubleshooting

## `GET /api/auth/me` returns 401

Check:

- token exists;
- token is not expired;
- in Swagger Authorize, paste raw token only;
- do not paste `Bearer <token>` into Swagger Authorize.

## Add-to-cart returns 404

Most likely causes:

- wrong product ID;
- product is inactive and hidden from public catalog;
- product was soft-deactivated (`IsActive = false`).

## Checkout returns 409

This is usually a business-rule failure:

- empty cart;
- insufficient stock;
- product became inactive after it was added to cart.

# Part 4 — Reviewer Checklist

Use this list to validate the portfolio flow end to end.

- [ ] API starts with Docker Compose or local PostgreSQL + `dotnet run`.
- [ ] Swagger opens at http://localhost:5000/swagger.
- [ ] Admin can log in and create an active product (SKU, price, stock, `isActive`).
- [ ] Public catalog lists only active products.
- [ ] User registers, logs in, and `GET /api/auth/me` works with JWT.
- [ ] User adds product to cart and updates quantity by cart item id.
- [ ] Checkout returns an order with snapshotted line items.
- [ ] Stock decreases and cart is empty after checkout.
- [ ] User sees only own orders; another user's order id returns 404.
- [ ] Negative cases from Part 2 return documented status codes.
- [ ] `dotnet test` passes.
- [ ] `./scripts/api-smoke-test.sh` passes against a running API.

Automated smoke test:

```bash
./scripts/api-smoke-test.sh
```
