#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "==> Starting Postgres (docker compose)"
docker compose up -d postgres

echo "==> Waiting for healthy Postgres..."
for i in $(seq 1 40); do
  if docker compose exec -T postgres pg_isready -U ct -d ct_modular >/dev/null 2>&1; then
    echo "Postgres is ready."
    break
  fi
  sleep 1
  if [[ $i -eq 40 ]]; then
    echo "Postgres did not become ready in time." >&2
    exit 1
  fi
done

docker compose exec -T postgres psql -U ct -d postgres -tc "SELECT 1 FROM pg_database WHERE datname='ct_modular_test'" | grep -q 1 \
  || docker compose exec -T postgres psql -U ct -d postgres -c "CREATE DATABASE ct_modular_test OWNER ct;"

echo "Databases: ct_modular, ct_modular_test"
echo "App:  Host=localhost;Port=5432;Database=ct_modular;Username=ct;Password=ct_dev_password"
echo "Test: Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password"
