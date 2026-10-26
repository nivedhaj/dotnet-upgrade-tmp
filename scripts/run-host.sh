#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

export ConnectionStrings__Postgres="${ConnectionStrings__Postgres:-Host=localhost;Port=5432;Database=ct_modular;Username=ct;Password=ct_dev_password}"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://localhost:5001}"

echo "==> Modules referenced by Ct.Host:"
grep -oE 'Modules\\Ct\.[A-Za-z]+\\Ct\.[A-Za-z]+\.csproj|Modules/Ct\.[A-Za-z]+/Ct\.[A-Za-z]+\.csproj' src/Ct.Host/Ct.Host.csproj \
  | sed 's|.*Modules[/\\]||;s|[/\\].*||' | sort -u | sed 's/^/  - /'

echo "==> Building Ct.Host"
dotnet build src/Ct.Host/Ct.Host.csproj -c Debug --nologo

echo "==> Running Ct.Host on $ASPNETCORE_URLS"
echo "    Health:  $ASPNETCORE_URLS/health"
echo "    Swagger: $ASPNETCORE_URLS/swagger"
dotnet run --project src/Ct.Host/Ct.Host.csproj -c Debug --no-build
