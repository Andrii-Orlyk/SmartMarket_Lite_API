# Swagger Testing Guide

Swagger URL in local Docker runtime:

```text
http://localhost:5000/swagger
```

## Auth note

Swagger UI Authorize popup expects only the raw JWT token.

Correct:

```text
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Incorrect:

```text
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

If Swagger generated curl contains `Authorization: Bearer Bearer`, clear Authorize and paste only the raw token.

## Development admin

In `Development`, log in as the seeded admin before admin endpoints:

```text
Email: admin@smartmarket.local
Password: Password123!
```

## Happy path

1. Register or login (or use Development admin for catalog management).
2. Authorize with raw JWT.
3. `GET /api/auth/me`.
4. Use admin token to create active product.
5. Use user token to add product to cart.
6. Update cart item quantity.
7. Checkout.
8. Get created order.

## Expected failures

- Missing token -> 401 `auth.unauthorized`.
- User on admin endpoint -> 403 `auth.forbidden`.
- Invalid product price -> 400.
- Quantity 0 -> 400.
- Empty cart checkout -> 409.
- Other user's order -> 404.
