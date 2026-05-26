# API Contract

Base URL for local Docker runtime:

```text
http://localhost:5000
```

Protected endpoints require:

```http
Authorization: Bearer <jwt-token>
```

Swagger UI Authorize popup expects only the raw JWT token value, without `Bearer` prefix.

## Auth

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

## Products

Public product endpoints:

```http
GET /api/products
GET /api/products/{id}
```

Admin product endpoints:

```http
POST   /api/admin/products
PUT    /api/admin/products/{id}
DELETE /api/admin/products/{id}
```

`DELETE` soft-deactivates the product (`IsActive = false`).

Product query example:

```http
GET /api/products?search=mouse&minPrice=10&maxPrice=100&page=1&pageSize=10
```

## Cart

```http
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
DELETE /api/cart
```

Cart item routes use `CartItem.Id`, not `Product.Id`.

## Checkout

```http
POST /api/checkout
```

## Orders

```http
GET /api/orders
GET /api/orders/{id}
```

## Admin orders

```http
GET   /api/admin/orders
PATCH /api/admin/orders/{id}/status
```

## Error response

All application errors use the same shape:

```json
{
  "statusCode": 400,
  "code": "validation.failed",
  "message": "Validation failed.",
  "errors": []
}
```

Unhandled server exceptions return:

```json
{
  "statusCode": 500,
  "code": "server.error",
  "message": "An unexpected error occurred.",
  "errors": []
}
```

Stack traces are not exposed in API responses.

Missing JWT on protected routes returns `401` with `auth.unauthorized`. Authenticated users without the required role receive `403` with `auth.forbidden`.

Common codes:

```text
server.error
auth.invalid_credentials
auth.email_exists
auth.unauthorized
auth.forbidden
product.not_found
product.inactive
product.invalid_price
product.invalid_stock
product.sku_exists
cart.empty
cart.item_not_found
cart.invalid_quantity
checkout.empty_cart
checkout.product_unavailable
checkout.insufficient_stock
order.not_found
order.invalid_status_transition
validation.failed
```
