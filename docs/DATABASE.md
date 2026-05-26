# Database Model

Database provider: PostgreSQL.

## Tables

```text
Users
Products
Carts
CartItems
Orders
OrderItems
```

## Users

Fields:

- Id PK
- Email unique
- PasswordHash
- FirstName optional
- LastName optional
- Role (`User`, `Admin` stored as string)
- CreatedAt

## Products

Fields:

- Id PK
- Name
- Description
- SKU unique
- Price decimal(18,2)
- StockQuantity int
- IsActive bool
- CreatedAt
- UpdatedAt

Indexes:

- unique index on `SKU`
- index on `IsActive`
- index on `Name`

## Carts

Fields:

- Id PK
- UserId FK unique
- CreatedAt
- UpdatedAt

Rules:

- one cart per user;
- cart created lazily.

## CartItems

Fields:

- Id PK
- CartId FK
- ProductId FK
- Quantity
- UnitPriceSnapshot decimal(18,2)

Indexes/constraints:

- unique index on `(CartId, ProductId)`
- `Quantity > 0`

## Orders

Fields:

- Id PK
- UserId FK
- OrderNumber unique
- Status
- TotalAmount decimal(18,2)
- CreatedAt

Indexes:

- index on `UserId`
- unique index on `OrderNumber`
- index on `Status`
- index on `CreatedAt`

## OrderItems

Fields:

- Id PK
- OrderId FK
- ProductId FK
- ProductNameSnapshot
- UnitPriceSnapshot decimal(18,2)
- Quantity
- LineTotal decimal(18,2)

## Checkout transaction

Checkout must create order, create order items, decrease product stock, and clear cart in a single transaction.

Pricing rule for v1:

- `CartItem.UnitPriceSnapshot` is set when the product is first added to the cart.
- `OrderItem.UnitPriceSnapshot` and `OrderItem.ProductNameSnapshot` are copied at checkout from the cart item and current product name.
- Later catalog price changes do not affect existing orders.

## Product delete behavior

`DELETE /api/admin/products/{id}` performs a soft deactivate (`IsActive = false`) to avoid breaking cart or order references.
