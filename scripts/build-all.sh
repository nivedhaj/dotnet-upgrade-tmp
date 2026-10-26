#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "==> Restore + build solution (all modules + tests in .sln)"
dotnet restore Ct.Modular.sln
dotnet build Ct.Modular.sln -c Release --nologo
echo "Build succeeded."
