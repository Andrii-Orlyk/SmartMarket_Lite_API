# Project Scope

## Project

SmartMarket Lite API is a backend API project for a small e-commerce system.

The project must prove the ability to build a business-oriented backend flow:

```text
catalog -> cart -> checkout -> order
```

## In scope

- User registration, login, and current-user endpoint.
- JWT Bearer authentication.
- Role-based access with `User` and `Admin`.
- Product catalog with admin management.
- Product fields: SKU, price, stock quantity, active flag.
- User cart.
- Cart item quantity updates.
- Checkout from cart.
- Order creation.
- Order item snapshots.
- Stock decrease after checkout.
- Cart clearing after checkout.
- Current user's order history.
- Admin order listing and status update.
- Validation and unified error responses.
- EF Core + PostgreSQL persistence.
- Unit tests, integration tests, and terminal smoke test.
- Docker Compose and GitHub Actions.

## Out of scope for v1

- Frontend implementation.
- UI/UX screens.
- Payment gateway integration.
- Product images and file upload.
- Categories.
- Vendor marketplace model.
- Discounts, coupons, tax, shipping, refunds.
- Distributed transactions, message queues, event sourcing.
- Cloud deployment.

## Why categories are out of scope

The portfolio goal is not to show the biggest possible e-commerce schema. The goal is to show the core buying flow clearly and correctly. Products are enough to demonstrate catalog, cart, checkout, stock, and orders.

Categories can be added later as an extension without changing the core checkout concept.

## Portfolio proof points

- Backend-only scope; no frontend or payment gateway in v1.
- Core flow: `catalog -> cart -> checkout -> order`.
- Order history uses product name and unit price snapshots; cart price is fixed at add-to-cart time.
- Checkout is one consistent operation (order, snapshots, stock decrease, cart clear).
- Automated tests: unit + integration (`dotnet test`) and GitHub Actions CI.
- Live runtime: Docker Compose + terminal smoke test against PostgreSQL.
