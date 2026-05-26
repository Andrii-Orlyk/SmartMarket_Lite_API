# Authentication and Authorization

## Authentication

The API uses JWT Bearer authentication.

Endpoints:

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

## Roles

```text
User
Admin
```

## Role rules

User:

- can view products;
- can manage own cart;
- can checkout;
- can view own orders.

Admin:

- can manage products;
- can view all orders;
- can update order status.

## Error responses

Missing or invalid JWT on protected endpoints returns:

```json
{
  "statusCode": 401,
  "code": "auth.unauthorized",
  "message": "Unauthorized.",
  "errors": []
}
```

Authenticated users without the required role (for example `User` on admin routes) receive:

```json
{
  "statusCode": 403,
  "code": "auth.forbidden",
  "message": "Access is forbidden.",
  "errors": []
}
```

## Access isolation

Users must not access another user's order. The API returns `404 order.not_found` rather than `403`, so existence of another user's order is not leaked.

## Development seed admin

In `Development` only, the API seeds a local admin for reviewer convenience:

```text
Email: admin@smartmarket.local
Password: Password123!
```

Do not use this account outside local development. Change credentials through configuration for other environments.
