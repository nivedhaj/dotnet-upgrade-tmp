#!/usr/bin/env bash
set -euo pipefail
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
