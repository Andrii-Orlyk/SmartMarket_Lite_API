# SmartMarket Lite API — PR / Release Log

## Final verification summary

- `dotnet restore`: passed
- `dotnet build`: passed
- `dotnet test`: passed — 11 unit + 33 integration
- `docker compose config`: passed
- live Docker smoke test: passed — 24/24
- public forbidden terms scan: must pass before push
- internal tracked files: must be none before push
- co-author trailers: must be none before push

---

## v0.1.0 — Project foundation

### Scope
Project skeleton and layered architecture.

### Included
- Solution file
- API/Application/Domain/Infrastructure projects
- Unit/Integration test projects
- Base README
- Project scope
- Roadmap
- Architecture draft

### Verification
Validated in final release pipeline.

### Result
Clean .NET backend project structure prepared.

### Reviewer value
Shows organized project foundation and layered backend setup.

---

## v0.2.0 — Domain model and application contracts

### Scope
E-commerce domain model and application contracts.

### Included
- User
- Product
- Cart
- CartItem
- Order
- OrderItem
- DTOs
- Validators
- Result / PagedResult
- ErrorCodes
- Application interfaces

### Verification
Validated in final release pipeline.

### Result
Core business model and application boundary created.

### Reviewer value
Shows ability to model e-commerce data and separate domain/application logic.

---

## v0.3.0 — Persistence layer

### Scope
EF Core + PostgreSQL persistence.

### Included
- SmartMarketDbContext
- Entity configurations
- Repositories
- UnitOfWork
- Migrations
- Constraints
- Indexes

### Verification
Validated by build, tests, and persistence schema tests.

### Result
SQL-backed persistence layer implemented.

### Reviewer value
Shows database-backed API skills, EF Core relationships, constraints, indexes, and schema evolution.

---

## v0.4.0 — Authentication, authorization and error handling

### Scope
JWT authentication, role-based access, and unified API errors.

### Included
- Register
- Login
- Me
- JWT token generation
- Password hashing
- Admin/User roles
- 401 `auth.unauthorized`
- 403 `auth.forbidden`
- 500 `server.error`
- ExceptionHandlingMiddleware
- Authorization result handler

### Verification
Auth, RBAC, and middleware tests passed in final release pipeline.

### Result
Protected API access and consistent error handling added.

### Reviewer value
Shows authentication, RBAC, claims-based access, and predictable API error contracts.

---

## v0.5.0 — Product catalog

### Scope
Product catalog and admin product management.

### Included
- Public product listing
- Product details
- Admin create/update/delete product
- SKU uniqueness
- Product validation
- Soft delete via `IsActive`

### Verification
Catalog integration tests passed in final release pipeline.

### Result
Product catalog feature implemented.

### Reviewer value
Shows product rules and admin-only management, not only generic CRUD.

---

## v0.6.0 — Shopping cart

### Scope
Authenticated user cart flow.

### Included
- Get cart
- Add product
- Update quantity
- Remove item
- Clear cart
- Quantity validation
- Inactive product blocking
- Stock check

### Verification
Cart integration tests passed in final release pipeline.

### Result
Shopping cart feature implemented.

### Reviewer value
Shows user-scoped state management and cart business rules.

---

## v0.7.0 — Checkout and orders

### Scope
Main e-commerce business flow.

### Included
- Checkout
- Order creation
- Order items
- ProductNameSnapshot
- UnitPriceSnapshot
- TotalAmount
- Stock decrease
- Cart clearing
- User order history
- Admin order management

### Verification
Checkout and order integration tests passed. Live smoke test confirmed the full flow.

### Result
`catalog → cart → checkout → order` implemented.

### Reviewer value
Shows real business-flow implementation, transaction thinking, stock validation, and snapshot modeling.

---

## v0.8.0 — Automated tests

### Scope
Automated unit and integration test coverage.

### Included
- Auth tests
- Catalog tests
- Cart tests
- Checkout tests
- Orders tests
- Middleware tests
- Persistence schema tests

### Verification
`dotnet test` passed — 11 unit + 33 integration.

### Result
Critical API behavior verified.

### Reviewer value
Shows testing discipline and QA-oriented backend development.

---

## v0.9.0 — Docker, CI and smoke testing

### Scope
Runtime and CI proof.

### Included
- Dockerfile
- docker-compose.yml
- PostgreSQL service
- GitHub Actions CI
- Terminal API smoke test

### Verification
- `docker compose config`: passed
- live Docker smoke test: 24/24 passed

### Result
Project can be built, tested, and run as a local Docker environment.

### Reviewer value
Shows practical run/test readiness and reviewer-friendly verification flow.

---

## v1.0.0 — Public portfolio release

### Scope
Final public GitHub-ready release.

### Included
- README
- API documentation
- Swagger guide
- Terminal testing guide
- Auth docs
- Database docs
- Known limitations
- Roadmap
- Release log

### Verification
Final release pipeline passed.

### Result
Clean public portfolio repository prepared.

### Reviewer value
Shows professional project presentation and honest portfolio documentation.
