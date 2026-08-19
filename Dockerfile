# syntax=docker/dockerfile:1

########################
# Build stage
########################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY Ct.Campus.sln ./
COPY src/ ./src/

# Restore using the solution (pulls in all referenced projects)
RUN dotnet restore Ct.Campus.sln

# Copy the rest of the source (database scripts, etc. if needed at build time)
COPY . .

# Publish the entry point project
WORKDIR /src/src/Ct.Host
RUN dotnet publish Ct.Host.csproj -c Release -o /app/publish --no-restore

########################
# Runtime stage
########################
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for healthcheck usage in docker-compose (optional, small footprint)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "Ct.Host.dll"]
