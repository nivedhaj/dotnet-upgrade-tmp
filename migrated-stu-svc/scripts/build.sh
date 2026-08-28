#!/usr/bin/env bash
set -euo pipefail

dotnet restore src/ct.csproj
dotnet build src/ct.csproj --configuration Release
dotnet test --configuration Release
