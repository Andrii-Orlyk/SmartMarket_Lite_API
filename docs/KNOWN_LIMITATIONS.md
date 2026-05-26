# Known Limitations

SmartMarket Lite API is a portfolio backend project, not a production commerce platform.

## Current limitations

- No frontend implementation.
- No real payment provider.
- No product images or file upload.
- No categories in v1.
- No vendor marketplace model.
- No discounts, coupons, taxes, shipping, refunds, or invoices.
- No distributed transactions.
- No inventory reservation system.
- No cloud deployment.
- Integration tests use EF Core InMemory via `WebApplicationFactory`. They validate API behavior quickly but do not fully validate PostgreSQL transaction semantics, constraints, or migration behavior.

## Intentional simplifications

Product delete is a soft deactivate, not a hard remove.

Cart stores `UnitPriceSnapshot` when a product is first added. Checkout uses that snapshot for order line pricing.

Checkout is implemented as one local database transaction. This is correct for the Lite scope. A larger production commerce system may need payment webhooks, reservation logic, outbox pattern, retries, and reconciliation workflows.

Order item snapshots are included because they are essential for correct order history.
