#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

export CT_TEST_POSTGRES="${CT_TEST_POSTGRES:-Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password}"

# Discover test projects under tests/**/*.csproj
mapfile -t ALL_PROJS < <(find tests -mindepth 2 -maxdepth 2 -name '*.csproj' | sort)

list_modules() {
  for p in "${ALL_PROJS[@]}"; do
    # tests/ct.fee.tests/ct.fee.tests.csproj  -> fee
    # tests/ct.acad.tests/Ct.Acad.Tests.csproj -> acad
    base="$(basename "$(dirname "$p")")"
    echo "$base" | sed -E 's/^ct\.//; s/\.tests$//'
  done
}

find_proj() {
  local key="$1"  # e.g. fee, acad, connect
  local key_l
  key_l="$(echo "$key" | tr '[:upper:]' '[:lower:]')"
  for p in "${ALL_PROJS[@]}"; do
    local dir base
    dir="$(basename "$(dirname "$p")")"
    base="$(basename "$p" .csproj)"
    dir_l="$(echo "$dir" | tr '[:upper:]' '[:lower:]')"
    base_l="$(echo "$base" | tr '[:upper:]' '[:lower:]')"
    if [[ "$dir_l" == "ct.${key_l}.tests" ]] || [[ "$base_l" == "ct.${key_l}.tests" ]] || [[ "$base_l" == "${key_l}.tests" ]]; then
      echo "$p"
      return 0
    fi
  done
  return 1
}

run_one_proj() {
  local proj="$1"
  echo ""
  echo "========== $proj =========="
  dotnet test "$proj" -c Release --nologo
}

MODULE="${1:-all}"

case "$MODULE" in
  -h|--help|help)
    echo "Usage: $0 [all|<module>]"
    echo "Discovered modules:"
    list_modules | sed 's/^/  - /'
    exit 0
    ;;
  all)
    if [[ ${#ALL_PROJS[@]} -eq 0 ]]; then
      echo "No test projects found under tests/" >&2
      exit 1
    fi
    for p in "${ALL_PROJS[@]}"; do
      run_one_proj "$p"
    done
    ;;
  *)
    proj="$(find_proj "$MODULE" || true)"
    if [[ -z "${proj:-}" ]]; then
      echo "Unknown module: $MODULE" >&2
      echo "Available:" >&2
      list_modules | sed 's/^/  /' >&2
      exit 1
    fi
    run_one_proj "$proj"
    ;;
esac

echo ""
echo "All requested tests finished."
