# Docker quick start

This package uses PostgreSQL 16 and .NET 10.

## Prerequisites

- Docker Engine
- Docker Compose v2 (`docker compose`)

## Start from a clean PostgreSQL database

If this is only a POC and you do not need existing PostgreSQL data:

```bash
docker compose down -v
docker compose up --build
```

The `-v` is important after changing `Database/schema.sql`, because PostgreSQL executes scripts in `/docker-entrypoint-initdb.d` only when the data directory is initialized for the first time.

## Run without deleting existing data

```bash
docker compose up --build
```

Do **not** use `docker compose down -v` if the PostgreSQL volume contains data you need.

## Check containers

```bash
docker compose ps
```

## Check application logs

```bash
docker compose logs -f app
```

## Check PostgreSQL logs

```bash
docker compose logs -f postgres
```

## Endpoints

The application is exposed on port 8080:

```text
http://localhost:8080
```

Swagger, if enabled by the application, is normally available at:

```text
http://localhost:8080/swagger
```

## PostgreSQL connection from the host

```text
Host=localhost
Port=5432
Database=campustrack
Username=campustrack
Password=campustrack
```

Inside the app container, use host `postgres`, not `localhost`.
