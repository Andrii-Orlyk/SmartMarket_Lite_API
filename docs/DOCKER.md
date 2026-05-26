# Docker

Docker Compose runs the API and PostgreSQL for local review.

## Start

From the repository root:

```bash
docker compose up --build
```

Services:

| Service | URL / port |
|---|---|
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |

The API container listens on port `8080` internally and is published as `5000` on the host.

## Development admin seed

When `ASPNETCORE_ENVIRONMENT=Development`, the API seeds a local admin account on startup:

```text
Email: admin@smartmarket.local
Password: Password123!
```

Use this account for admin product and order endpoints during manual or Swagger testing.

## Stop

```bash
docker compose down
```

To remove the database volume:

```bash
docker compose down -v
```

## Configuration

Connection string and JWT settings come from `docker-compose.yml` and `src/SmartMarket.Api/appsettings.json`.

Change JWT secrets before any non-local use.
